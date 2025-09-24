using Core.Interfaces;
using Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf"
    };

    private readonly ILogger<FileStorageService> _logger;
    private readonly FileStorageOptions _options;

    public FileStorageService(ILogger<FileStorageService> logger, IOptions<FileStorageOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
    {
        try
        {
            // Validate content type and extension
            var fileExtension = Path.GetExtension(fileName);
            if (!AllowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException($"File extension not allowed: {fileExtension}");
            }
            if (!AllowedContentTypes.Contains(contentType))
            {
                throw new InvalidOperationException($"Content-Type not allowed: {contentType}");
            }

            // Basic magic number validation for common types
            if (!await ValidateMagicAsync(fileStream, contentType))
            {
                throw new InvalidOperationException("File content does not match declared content type");
            }

            var uploadPath = Path.Combine(_options.UploadPath, folder ?? "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            fileStream.Position = 0; // reset after magic check
            using var fileStreamToSave = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamToSave);

            var relativePath = Path.Combine(folder ?? "uploads", uniqueFileName).Replace("\\", "/");
            var fileUrl = $"{_options.BaseUrl}/{relativePath}";

            _logger.LogInformation("File uploaded successfully: {FileName} -> {FileUrl}", fileName, fileUrl);
            return fileUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file: {FileName}", fileName);
            throw;
        }
    }

    private static async Task<bool> ValidateMagicAsync(Stream stream, string contentType)
    {
        stream.Position = 0;
        var buffer = new byte[12];
        var read = await stream.ReadAsync(buffer, 0, buffer.Length);
        if (read < 4) return false;

        // JPEG FF D8 FF
        if (contentType == "image/jpeg")
        {
            return buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
        }
        // PNG 89 50 4E 47 0D 0A 1A 0A
        if (contentType == "image/png")
        {
            return buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47 && buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A;
        }
        // GIF 47 49 46 38
        if (contentType == "image/gif")
        {
            return buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38;
        }
        // WEBP RIFF....WEBP
        if (contentType == "image/webp")
        {
            return buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                   buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50;
        }
        // PDF %PDF
        if (contentType == "application/pdf")
        {
            return buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46;
        }
        return false;
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var relativePath = fileUrl.Replace(_options.BaseUrl + "/", "").Replace("/", "\\");
            var filePath = Path.Combine(_options.UploadPath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return true;
            }

            _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<Stream?> GetFileAsync(string fileUrl)
    {
        try
        {
            var relativePath = fileUrl.Replace(_options.BaseUrl + "/", "").Replace("/", "\\");
            var filePath = Path.Combine(_options.UploadPath, relativePath);

            if (File.Exists(filePath))
            {
                return new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }

            _logger.LogWarning("File not found: {FilePath}", filePath);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file: {FileUrl}", fileUrl);
            return null;
        }
    }

    public Task<string> GetFileUrlAsync(string filePath)
    {
        var relativePath = filePath.Replace(_options.UploadPath, "").Replace("\\", "/").TrimStart('/');
        return Task.FromResult($"{_options.BaseUrl}/{relativePath}");
    }

    public Task<bool> FileExistsAsync(string fileUrl)
    {
        try
        {
            var relativePath = fileUrl.Replace(_options.BaseUrl + "/", "").Replace("/", "\\");
            var filePath = Path.Combine(_options.UploadPath, relativePath);
            return Task.FromResult(File.Exists(filePath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check file existence: {FileUrl}", fileUrl);
            return Task.FromResult(false);
        }
    }
}

public class FileStorageOptions
{
    public string UploadPath { get; set; } = "wwwroot/uploads";
    public string BaseUrl { get; set; } = "https://localhost:7001";
}
