using FileManager.Server.Data;

namespace FileManager.Server.Services;

public class FileStorageService
{
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");

    public FileStorageService()
    {
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> SaveFileAsync(IFormFile file, string storedName)
    {
        var filePath = Path.Combine(_storagePath, storedName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return filePath;
    }

    public string GetFilePath(string storedName)
    {
        return Path.Combine(_storagePath, storedName);
    }
}