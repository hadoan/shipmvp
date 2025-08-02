using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShipMvp.Application.Files;

namespace ShipMvp.Api.Controllers;

/// <summary>
/// File management API controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileAppService _fileAppService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileAppService fileAppService,
        ILogger<FilesController> logger)
    {
        _fileAppService = fileAppService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<FileUploadResultDto>> UploadFile(IFormFile file, [FromForm] FileUploadRequest request)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        var uploadDto = new FileUploadDto
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Size = file.Length,
            ContainerName = request.ContainerName,
            IsPublic = request.IsPublic,
            Tags = request.Tags
        };

        using var stream = file.OpenReadStream();
        var result = await _fileAppService.UploadAsync(uploadDto, stream);

        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result);
    }

    /// <summary>
    /// Download a file
    /// </summary>
    [HttpGet("{fileId}/download")]
    public async Task<IActionResult> DownloadFile(Guid fileId)
    {
        try
        {
            var file = await _fileAppService.DownloadAsync(fileId);
            return File(file.FileStream, file.ContentType, file.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound("File not found");
        }
    }

    /// <summary>
    /// Get file information
    /// </summary>
    [HttpGet("{fileId}")]
    public async Task<ActionResult<FileDto>> GetFile(Guid fileId)
    {
        var file = await _fileAppService.GetAsync(fileId);
        if (file == null)
        {
            return NotFound("File not found");
        }
        return Ok(file);
    }

    /// <summary>
    /// Get list of files with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<FileListDto>> GetFiles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? containerName = null)
    {
        var files = await _fileAppService.GetListAsync(page, pageSize, userId, containerName);
        return Ok(files);
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{fileId}")]
    public async Task<IActionResult> DeleteFile(Guid fileId)
    {
        await _fileAppService.DeleteAsync(fileId);
        return NoContent();
    }

    /// <summary>
    /// Get signed URL for file access
    /// </summary>
    [HttpGet("{fileId}/signed-url")]
    public async Task<ActionResult<string>> GetSignedUrl(Guid fileId, [FromQuery] int expirationHours = 1)
    {
        try
        {
            var expiration = TimeSpan.FromHours(expirationHours);
            var signedUrl = await _fileAppService.GetSignedUrlAsync(fileId, expiration);
            return Ok(new { SignedUrl = signedUrl });
        }
        catch (FileNotFoundException)
        {
            return NotFound("File not found");
        }
    }
}

/// <summary>
/// Request model for file upload
/// </summary>
public class FileUploadRequest
{
    public string? ContainerName { get; set; }
    public bool IsPublic { get; set; }
    public string? Tags { get; set; }
}
