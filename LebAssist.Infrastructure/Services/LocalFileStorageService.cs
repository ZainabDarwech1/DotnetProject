using LebAssist.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LebAssist.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<LocalFileStorageService> _logger;

        public string[] AllowedImageExtensions => new[] { ".jpg", ".jpeg", ".png", ".webp" };
        public long MaxFileSizeBytes => 5 * 1024 * 1024; // 5MB

        public LocalFileStorageService(
            IWebHostEnvironment webHostEnvironment,
            ILogger<LocalFileStorageService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // R11.2: Validate image file
        public bool ValidateImageFile(string fileName, long fileSize)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid file extension: {Extension}", extension);
                return false;
            }

            if (fileSize > MaxFileSizeBytes)
            {
                _logger.LogWarning("File size {Size} exceeds maximum {MaxSize}", fileSize, MaxFileSizeBytes);
                return false;
            }

            return true;
        }

        // R11.1: Upload from IFormFile
        public async Task<string?> UploadFileAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return null;

                if (!ValidateImageFile(file.FileName, file.Length))
                    return null;

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                return await SaveFileAsync(memoryStream.ToArray(), file.FileName, folder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to folder {Folder}", folder);
                return null;
            }
        }

        // R11.1: Upload from Stream
        public async Task<string?> UploadFileAsync(Stream stream, string fileName, string folder)
        {
            try
            {
                if (stream == null || stream.Length == 0)
                    return null;

                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                return await SaveFileAsync(memoryStream.ToArray(), fileName, folder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading stream to folder {Folder}", folder);
                return null;
            }
        }

        // R11.6: Save with unique filename (security - never use original name)
        public async Task<string?> SaveFileAsync(byte[] fileData, string fileName, string folder)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // R11.6: Generate unique filename using GUID
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var uniqueFileName = $"{timestamp}_{Guid.NewGuid():N}{extension}";

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await File.WriteAllBytesAsync(filePath, fileData);

                var relativePath = $"/uploads/{folder}/{uniqueFileName}";
                _logger.LogInformation("File saved successfully: {Path}", relativePath);

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file to folder {Folder}", folder);
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                // R11.6: Prevent directory traversal
                if (filePath.Contains("..") || filePath.Contains("~"))
                {
                    _logger.LogWarning("Directory traversal attempt detected: {Path}", filePath);
                    return false;
                }

                var physicalPath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(physicalPath))
                {
                    await Task.Run(() => File.Delete(physicalPath));
                    _logger.LogInformation("File deleted successfully: {Path}", filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
                return false;
            }
        }

        public string GetFileUrl(string filePath)
        {
            return filePath;
        }

        // R11.6: Get file stream for secure serving
        public async Task<Stream?> GetFileStreamAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return null;

                // R11.6: Prevent directory traversal
                if (filePath.Contains("..") || filePath.Contains("~"))
                {
                    _logger.LogWarning("Directory traversal attempt detected: {Path}", filePath);
                    return null;
                }

                var physicalPath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(physicalPath))
                {
                    var bytes = await File.ReadAllBytesAsync(physicalPath);
                    return new MemoryStream(bytes);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file {FilePath}", filePath);
                return null;
            }
        }
    }
}