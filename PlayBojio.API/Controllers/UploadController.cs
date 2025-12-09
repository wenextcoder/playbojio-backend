using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayBojio.API.Services;

namespace PlayBojio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IR2StorageService _storageService;
    private readonly ILogger<UploadController> _logger;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public UploadController(IR2StorageService storageService, ILogger<UploadController> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            // Validate file size
            if (file.Length > MaxFileSize)
                return BadRequest(new { message = $"File size exceeds maximum allowed size of {MaxFileSize / 1024 / 1024}MB" });

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return BadRequest(new { message = "Invalid file type. Only image files are allowed." });

            // Validate content type
            if (!file.ContentType.StartsWith("image/"))
                return BadRequest(new { message = "Invalid content type. Only images are allowed." });

            // Upload to R2
            using var stream = file.OpenReadStream();
            var url = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType);

            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, new { message = "Failed to upload image" });
        }
    }

    [HttpDelete("image")]
    public async Task<IActionResult> DeleteImage([FromQuery] string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest(new { message = "URL is required" });

            var success = await _storageService.DeleteFileAsync(url);
            
            if (!success)
                return NotFound(new { message = "File not found or already deleted" });

            return Ok(new { message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image");
            return StatusCode(500, new { message = "Failed to delete image" });
        }
    }
}

