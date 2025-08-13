using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShipMvp.Domain.Files;
using ShipMvp.Core;
using ShipMvp.Core.Abstractions;
using ShipMvp.Core.Events;

namespace ShipMvp.Application.Files;

/// <summary>
/// Application service for file management operations
/// </summary>
public interface IFileAppService
{
    Task<FileUploadResultDto> UploadAsync(FileUploadDto input, Stream fileStream, CancellationToken cancellationToken = default);
    Task<FileDownloadDto> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<FileDto?> GetAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<FileListDto> GetListAsync(int page = 1, int pageSize = 10, Guid? userId = null, string? containerName = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<string> GetSignedUrlAsync(Guid fileId, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}

public class FileAppService : IFileAppService
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEventBus _eventBus;
    private readonly ILogger<FileAppService> _logger;

    private const string DEFAULT_CONTAINER = "shipmvp-files";
    private const int MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB

    public FileAppService(
        IFileRepository fileRepository,
        IFileStorageService fileStorageService,
        IEventBus eventBus,
        ILogger<FileAppService> logger)
    {
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<FileUploadResultDto> UploadAsync(
        FileUploadDto input,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting file upload: {FileName}, Size: {Size}", input.FileName, input.Size);

            // Validate file size
            if (input.Size > MAX_FILE_SIZE)
            {
                return new FileUploadResultDto
                {
                    Success = false,
                    ErrorMessage = $"File size exceeds maximum allowed size of {MAX_FILE_SIZE / (1024 * 1024)}MB"
                };
            }

            // Generate unique file name
            var fileId = Guid.NewGuid();
            var extension = Path.GetExtension(input.FileName);
            var uniqueFileName = $"{fileId}{extension}";
            var containerName = string.IsNullOrEmpty(input.ContainerName) ? DEFAULT_CONTAINER : input.ContainerName;

            // Calculate hash
            var hash = await CalculateHashAsync(fileStream);
            fileStream.Position = 0; // Reset stream position

            // Upload to cloud storage
            var storagePath = await _fileStorageService.UploadAsync(
                containerName,
                uniqueFileName,
                fileStream,
                input.ContentType,
                input.IsPublic,
                cancellationToken);

            // Create file entity
            var file = new Domain.Files.File(
                fileId,
                containerName,
                uniqueFileName,
                input.FileName,
                input.ContentType,
                input.Size,
                storagePath,
                null, // TODO: Get current user ID
                input.IsPublic);

            file.Hash = hash;
            if (!string.IsNullOrEmpty(input.Tags))
            {
                file.UpdateTags(input.Tags);
            }

            // Set public URL if public
            if (input.IsPublic)
            {
                var publicUrl = _fileStorageService.GetPublicUrl(containerName, uniqueFileName);
                file.SetPublicUrl(publicUrl);
            }

            // Save to database
            var savedFile = await _fileRepository.InsertAsync(file, cancellationToken);

            // Publish event
            await _eventBus.PublishAsync(new EntityCreatedEventData<Domain.Files.File>(savedFile));

            _logger.LogInformation("File uploaded successfully: {FileId}", fileId);

            return new FileUploadResultDto
            {
                FileId = fileId,
                FileName = input.FileName,
                PublicUrl = savedFile.PublicUrl,
                Size = input.Size,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", input.FileName);
            return new FileUploadResultDto
            {
                Success = false,
                ErrorMessage = "An error occurred while uploading the file"
            };
        }
    }

    public async Task<FileDownloadDto> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading file: {FileId}", fileId);

        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.IsDeleted)
        {
            throw new FileNotFoundException($"File with ID {fileId} not found");
        }

        var fileStream = await _fileStorageService.DownloadAsync(
            file.ContainerName,
            file.FileName,
            cancellationToken);

        return new FileDownloadDto
        {
            FileName = file.OriginalFileName,
            ContentType = file.MimeType,
            FileStream = fileStream
        };
    }

    public async Task<FileDto?> GetAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.IsDeleted)
        {
            return null;
        }

        return MapToDto(file);
    }

    public async Task<FileListDto> GetListAsync(
        int page = 1,
        int pageSize = 10,
        Guid? userId = null,
        string? containerName = null,
        CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;
        var (files, totalCount) = await _fileRepository.GetPaginatedAsync(
            skip, pageSize, userId, containerName, cancellationToken);

        return new FileListDto
        {
            Files = files.Select(MapToDto),
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting file: {FileId}", fileId);

        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.IsDeleted)
        {
            return;
        }

        // Delete from cloud storage
        await _fileStorageService.DeleteAsync(file.ContainerName, file.FileName, cancellationToken);

        // Soft delete in database
        await _fileRepository.DeleteAsync(fileId, cancellationToken);

        // Publish event
        await _eventBus.PublishAsync(new EntityDeletedEventData<Domain.Files.File>(file));

        _logger.LogInformation("File deleted successfully: {FileId}", fileId);
    }

    public async Task<string> GetSignedUrlAsync(
        Guid fileId,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.IsDeleted)
        {
            throw new FileNotFoundException($"File with ID {fileId} not found");
        }

        var exp = expiration ?? TimeSpan.FromHours(1);
        return await _fileStorageService.GetSignedUrlAsync(
            file.ContainerName,
            file.FileName,
            exp,
            cancellationToken);
    }

    private static FileDto MapToDto(Domain.Files.File file)
    {
        return new FileDto
        {
            Id = file.Id,
            ContainerName = file.ContainerName,
            FileName = file.FileName,
            OriginalFileName = file.OriginalFileName,
            MimeType = file.MimeType,
            Size = file.Size,
            Hash = file.Hash,
            StoragePath = file.StoragePath,
            UserId = file.UserId,
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt,
            PublicUrl = file.PublicUrl,
            IsPublic = file.IsPublic,
            Tags = file.Tags
        };
    }

    private static async Task<string> CalculateHashAsync(Stream stream)
    {
        using var md5 = MD5.Create();
        var hash = await md5.ComputeHashAsync(stream);
        return Convert.ToBase64String(hash);
    }
}
