using System;

namespace ShipMvp.Application.Files;

/// <summary>
/// DTO for file upload request
/// </summary>
public class FileUploadDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? ContainerName { get; set; }
    public bool IsPublic { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// DTO for file information
/// </summary>
public class FileDto
{
    public Guid Id { get; set; }
    public string ContainerName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? Hash { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? PublicUrl { get; set; }
    public bool IsPublic { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// DTO for file list with pagination
/// </summary>
public class FileListDto
{
    public IEnumerable<FileDto> Files { get; set; } = [];
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// DTO for file download
/// </summary>
public class FileDownloadDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public Stream FileStream { get; set; } = Stream.Null;
}

/// <summary>
/// DTO for file upload result
/// </summary>
public class FileUploadResultDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? PublicUrl { get; set; }
    public long Size { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
