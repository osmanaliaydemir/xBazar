namespace Core.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null);
    Task<bool> DeleteFileAsync(string fileUrl);
    Task<Stream?> GetFileAsync(string fileUrl);
    Task<string> GetFileUrlAsync(string filePath);
    Task<bool> FileExistsAsync(string fileUrl);
}
