using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ShipMvp.Domain.Files;

/// <summary>
/// File storage service interface for cloud storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to cloud storage
    /// </summary>
    Task<string> UploadAsync(
        string containerName,
        string fileName,
        Stream fileStream,
        string contentType,
        bool isPublic = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from cloud storage
    /// </summary>
    Task<Stream> DownloadAsync(
        string containerName,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from cloud storage
    /// </summary>
    Task DeleteAsync(
        string containerName,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a signed URL for file access
    /// </summary>
    Task<string> GetSignedUrlAsync(
        string containerName,
        string fileName,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the public URL for a file
    /// </summary>
    string GetPublicUrl(string containerName, string fileName);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    Task<bool> ExistsAsync(
        string containerName,
        string fileName,
        CancellationToken cancellationToken = default);
}
