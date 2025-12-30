using LebAssist.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LebAssist.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileStorageService _fileStorage;
        private readonly IClientService _clientService;
        private readonly ILogger<FileController> _logger;

        public FileController(
            IFileStorageService fileStorage,
            IClientService clientService,
            ILogger<FileController> logger)
        {
            _fileStorage = fileStorage;
            _clientService = clientService;
            _logger = logger;
        }

        /// <summary>
        /// R11.4: Upload profile photo
        /// </summary>
        [HttpPost("profile-photo")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB limit
        public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, error = "No file uploaded." });

                if (!_fileStorage.ValidateImageFile(file.FileName, file.Length))
                    return BadRequest(new { success = false, error = $"Invalid file. Allowed: {string.Join(", ", _fileStorage.AllowedImageExtensions)}. Max size: 5MB." });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var profile = await _clientService.GetProfileAsync(userId);
                if (profile == null)
                    return Unauthorized();

                if (!string.IsNullOrEmpty(profile.ProfilePhotoPath))
                {
                    await _fileStorage.DeleteFileAsync(profile.ProfilePhotoPath);
                }

                var photoPath = await _fileStorage.UploadFileAsync(file, "profiles");
                if (photoPath == null)
                    return StatusCode(500, new { success = false, error = "Error saving file." });

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                await _clientService.UpdateProfilePhotoAsync(userId, memoryStream.ToArray(), file.FileName);

                _logger.LogInformation("Profile photo uploaded for user {UserId}", userId);

                return Ok(new
                {
                    success = true,
                    photoUrl = photoPath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile photo");
                return StatusCode(500, new { success = false, error = "Error uploading photo." });
            }
        }

        /// <summary>
        /// R11.5: Upload portfolio photos (multiple) - Provider only
        /// </summary>
        [HttpPost("portfolio")]
        [Authorize(Roles = "Provider")]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB for multiple files
        public async Task<IActionResult> UploadPortfolioPhotos(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest(new { success = false, error = "No files uploaded." });

                if (files.Count > 20)
                    return BadRequest(new { success = false, error = "Maximum 20 files allowed." });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var profile = await _clientService.GetProfileAsync(userId);
                if (profile == null)
                    return Unauthorized();

                var uploadedPhotos = new List<object>();
                var errors = new List<string>();

                foreach (var file in files)
                {
                    try
                    {
                        if (!_fileStorage.ValidateImageFile(file.FileName, file.Length))
                        {
                            errors.Add($"{file.FileName}: Invalid file type or size");
                            continue;
                        }

                        var photoPath = await _fileStorage.UploadFileAsync(file, $"portfolio/{profile.ClientId}");
                        if (photoPath == null)
                        {
                            errors.Add($"{file.FileName}: Upload failed");
                            continue;
                        }

                        uploadedPhotos.Add(new
                        {
                            fileName = file.FileName,
                            url = photoPath
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing file: {FileName}", file.FileName);
                        errors.Add($"{file.FileName}: Processing error");
                    }
                }

                _logger.LogInformation("Portfolio photos uploaded for user {UserId}. Success: {Count}", userId, uploadedPhotos.Count);

                return Ok(new
                {
                    success = true,
                    uploadedCount = uploadedPhotos.Count,
                    photos = uploadedPhotos,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading portfolio photos");
                return StatusCode(500, new { success = false, error = "Error uploading photos." });
            }
        }

        /// <summary>
        /// R11.5: Delete portfolio photo
        /// </summary>
        [HttpDelete("portfolio")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> DeletePortfolioPhoto([FromQuery] string photoUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(photoUrl))
                    return BadRequest(new { success = false, error = "Photo URL required." });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var profile = await _clientService.GetProfileAsync(userId);
                if (profile == null)
                    return Unauthorized();

                if (!photoUrl.Contains($"/portfolio/{profile.ClientId}/"))
                {
                    _logger.LogWarning("Unauthorized delete attempt: User {UserId} tried to delete {Url}", userId, photoUrl);
                    return Forbid();
                }

                var result = await _fileStorage.DeleteFileAsync(photoUrl);

                _logger.LogInformation("Portfolio photo deleted for user {UserId}: {Url}", userId, photoUrl);

                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio photo");
                return StatusCode(500, new { success = false, error = "Error deleting photo." });
            }
        }

        /// <summary>
        /// R11.6: Serve files securely with authorization
        /// </summary>
        [HttpGet("secure/{folder}/{*fileName}")]
        public async Task<IActionResult> GetSecureFile(string folder, string fileName)
        {
            try
            {
                if (fileName.Contains("..") || fileName.Contains("~") || folder.Contains(".."))
                {
                    _logger.LogWarning("Directory traversal attempt: {Folder}/{FileName}", folder, fileName);
                    return BadRequest("Invalid file path.");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var filePath = $"/uploads/{folder}/{fileName}";
                var stream = await _fileStorage.GetFileStreamAsync(filePath);

                if (stream == null)
                    return NotFound();

                var contentType = GetContentType(fileName);

                _logger.LogInformation("Secure file access: User {UserId} accessed {FilePath}", userId, filePath);

                return File(stream, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving file {Folder}/{FileName}", folder, fileName);
                return StatusCode(500);
            }
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}