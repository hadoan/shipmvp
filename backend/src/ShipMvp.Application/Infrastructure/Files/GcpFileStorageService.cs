
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShipMvp.Domain.Files;
using Google.Cloud.Storage.V1;
using ShipMvp.Application.Infrastructure.Gcp;
using ShipMvp.Core.Abstractions;

namespace ShipMvp.Application.Infrastructure.Files;

/// <summary>
/// Google Cloud Storage implementation of IFileStorageService
/// </summary>
public class GcpFileStorageService : IFileStorageService
{

    private readonly ILogger<GcpFileStorageService> _logger;
    private readonly string _projectId;
    private readonly string _defaultBucket;
    private readonly StorageClient _storageClient;
    private readonly string? _credentialsPath;

    public GcpFileStorageService(
        IConfiguration configuration,
        ILogger<GcpFileStorageService> logger)
    {
        _logger = logger;
        _projectId = configuration["Gcp:ProjectId"] ?? "demo-project";
        _defaultBucket = configuration["Gcp:Storage:DefaultBucket"] ?? "shipmvp-files";
        _credentialsPath = configuration["Gcp:CredentialsPath"];
        var credential = GcpCredentialFactory.Create(configuration);
        _storageClient = StorageClient.Create(credential);
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
            _logger.LogInformation("Uploading file to GCS: {Container}/{FileName}", containerName, fileName);
            var bucketName = containerName ?? _defaultBucket;
            await _storageClient.UploadObjectAsync(
                bucketName,
                fileName,
                contentType,
                fileStream,
                cancellationToken: cancellationToken
            );
            var storagePath = $"gs://{bucketName}/{fileName}";
            _logger.LogInformation("File uploaded successfully to GCS: {StoragePath}", storagePath);
            return storagePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to GCS: {Container}/{FileName}", containerName, fileName);
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
            _logger.LogInformation("Downloading file from GCS: {Container}/{FileName}", containerName, fileName);
            var bucketName = containerName ?? _defaultBucket;
            var ms = new MemoryStream();
            await _storageClient.DownloadObjectAsync(
                bucketName,
                fileName,
                ms,
                cancellationToken: cancellationToken
            );
            ms.Position = 0;
            return ms;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from GCS: {Container}/{FileName}", containerName, fileName);
            throw;
        }
    }

    public async Task DeleteAsync(
        string containerName,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting file from GCS: {Container}/{FileName}", containerName, fileName);
            var bucketName = containerName ?? _defaultBucket;
            await _storageClient.DeleteObjectAsync(
                bucketName,
                fileName,
                cancellationToken: cancellationToken
            );
            _logger.LogInformation("File deleted successfully from GCS: {Container}/{FileName}", containerName, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from GCS: {Container}/{FileName}", containerName, fileName);
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
            // For demo: return a gs:// URL (real signed URL requires IAM credentials and URL signer)
            var signedUrl = $"gs://{bucketName}/{fileName}?expires={DateTime.UtcNow.Add(expiration):yyyy-MM-ddTHH:mm:ssZ}";
            _logger.LogInformation("Generated GCS signed URL for: {Container}/{FileName}", containerName, fileName);
            return Task.FromResult(signedUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signed URL for GCS: {Container}/{FileName}", containerName, fileName);
            throw;
        }
    }

    public string GetPublicUrl(string containerName, string fileName)
    {
        var bucketName = containerName ?? _defaultBucket;
        // Return a public GCS URL (if object is public)
        return $"https://storage.googleapis.com/{bucketName}/{fileName}";
    }

    public async Task<bool> ExistsAsync(
        string containerName,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketName = containerName ?? _defaultBucket;
            var obj = await _storageClient.GetObjectAsync(bucketName, fileName, cancellationToken: cancellationToken);
            return obj != null;
        }
        catch (Google.GoogleApiException ex) when (ex.Error.Code == 404)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}
