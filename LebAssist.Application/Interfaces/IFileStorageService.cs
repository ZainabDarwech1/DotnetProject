using Microsoft.AspNetCore.Http;

namespace LebAssist.Application.Interfaces
{
    public interface IFileStorageService
    {
        // Upload methods
        Task<string?> UploadFileAsync(IFormFile file, string folder);
        Task<string?> UploadFileAsync(Stream stream, string fileName, string folder);
        Task<string?> SaveFileAsync(byte[] fileData, string fileName, string folder);

        // Delete and retrieve
        Task<bool> DeleteFileAsync(string filePath);
        string GetFileUrl(string filePath);
        Task<Stream?> GetFileStreamAsync(string filePath);

        // Validation
        bool ValidateImageFile(string fileName, long fileSize);
        string[] AllowedImageExtensions { get; }
        long MaxFileSizeBytes { get; }
    }
}