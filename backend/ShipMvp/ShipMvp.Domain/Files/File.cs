using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ShipMvp.Core;
using ShipMvp.Core.Entities;

namespace ShipMvp.Domain.Files;

/// <summary>
/// Represents a file in the system
/// </summary>
public class File : Entity<Guid>
{
    /// <summary>
    /// Container name (bucket name in GCP)
    /// </summary>
    public string ContainerName { get; set; } = string.Empty;

    /// <summary>
    /// File name with extension
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Original file name when uploaded
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the file
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public decimal FileSize { get; set; } = 0;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// MD5 hash of the file content
    /// </summary>
    public string? Hash { get; set; }

    /// <summary>
    /// Full path/key in the storage
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// User ID who uploaded the file
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// URL to access the file (if public)
    /// </summary>
    public string? PublicUrl { get; set; }

    /// <summary>
    /// Whether the file is publicly accessible
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Tags associated with the file
    /// </summary>
    public string? Tags { get; set; }

    protected File() : base(Guid.Empty) { }

    public File(
        Guid id,
        string containerName,
        string fileName,
        string originalFileName,
        string mimeType,
        long size,
        string storagePath,
        Guid? userId = null,
        bool isPublic = false)
        : base(id)
    {
        ContainerName = containerName ?? throw new ArgumentNullException(nameof(containerName));
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        OriginalFileName = originalFileName ?? throw new ArgumentNullException(nameof(originalFileName));
        MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
        Size = size;
        StoragePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
        UserId = userId;
        IsPublic = isPublic;
    }

    public void SetPublicUrl(string publicUrl)
    {
        PublicUrl = publicUrl;
    }

    public void UpdateTags(string tags)
    {
        Tags = tags;
    }
}
