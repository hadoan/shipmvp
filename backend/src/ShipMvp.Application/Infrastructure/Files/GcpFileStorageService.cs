using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShipMvp.Domain.Files;

namespace ShipMvp.Application.Infrastructure.Files;

/// <summary>
/// Mock Google Cloud Storage implementation of IFileStorageService for demo purposes
/// TODO: Replace with actual GCP implementation when GCP credentials are configured
/// </summary>
public class GcpFileStorageService : IFileStorageService
{
    private readonly ILogger<GcpFileStorageService> _logger;
    private readonly string _projectId;
    private readonly string _defaultBucket;
    private readonly string _baseStoragePath;

    public GcpFileStorageService(
        IConfiguration configuration,
        ILogger<GcpFileStorageService> logger)
    {
        _logger = logger;
        _projectId = configuration["GCP:ProjectId"] ?? "demo-project";
        _defaultBucket = configuration["GCP:Storage:DefaultBucket"] ?? "shipmvp-files";

        // For demo purposes, use local file system
        _baseStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(_baseStoragePath);
    }

    public async Task<string> UploadAsync(
        string containerName,
        string fileName,
        Stream fileStream,
        string contentType,
        bool isPublic = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Uploading file to mock GCS: {Container}/{FileName}", containerName, fileName);

            var bucketName = containerName ?? _defaultBucket;
            var bucketPath = Path.Combine(_baseStoragePath, bucketName);
            Directory.CreateDirectory(bucketPath);

            var filePath = Path.Combine(bucketPath, fileName);

            using var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

            var storagePath = $"gs://{bucketName}/{fileName}";
            _logger.LogInformation("File uploaded successfully to mock storage: {StoragePath}", storagePath);

            return storagePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to mock GCS: {Container}/{FileName}", containerName, fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadAsync(
        string containerName,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Downloading file from mock GCS: {Container}/{FileName}", containerName, fileName);

            var bucketName = containerName ?? _defaultBucket;
            var filePath = Path.Combine(_baseStoragePath, bucketName, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {fileName}");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath, cancellationToken);
            return new MemoryStream(fileBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from mock GCS: {Container}/{FileName}", containerName, fileName);
            throw;
        }
    }

    public Task DeleteAsync(
        string containerName,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting file from mock GCS: {Container}/{FileName}", containerName, fileName);

            var bucketName = containerName ?? _defaultBucket;
            var filePath = Path.Combine(_baseStoragePath, bucketName, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _logger.LogInformation("File deleted successfully from mock GCS: {Container}/{FileName}", containerName, fileName);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from mock GCS: {Container}/{FileName}", containerName, fileName);
            throw;
        }
    }

    public Task<string> GetSignedUrlAsync(
        string containerName,
        string fileName,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketName = containerName ?? _defaultBucket;

            // For demo purposes, return a mock signed URL
            var signedUrl = $"http://localhost:5000/api/files/mock-signed/{bucketName}/{fileName}?expires={DateTime.UtcNow.Add(expiration):yyyy-MM-ddTHH:mm:ssZ}";

            _logger.LogInformation("Generated mock signed URL for: {Container}/{FileName}", containerName, fileName);
            return Task.FromResult(signedUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signed URL for mock GCS: {Container}/{FileName}", containerName, fileName);
            throw;
        }
    }

    public string GetPublicUrl(string containerName, string fileName)
    {
        var bucketName = containerName ?? _defaultBucket;
        // For demo purposes, return a mock public URL
        return $"http://localhost:5000/api/files/public/{bucketName}/{fileName}";
    }

    public Task<bool> ExistsAsync(
        string containerName,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketName = containerName ?? _defaultBucket;
            var filePath = Path.Combine(_baseStoragePath, bucketName, fileName);
            return Task.FromResult(System.IO.File.Exists(filePath));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
