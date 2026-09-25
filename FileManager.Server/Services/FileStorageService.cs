namespace FileManager.Server.Services;

/// <summary>
/// Сервис для физического сохранения и чтения файлов на диске сервера.
/// </summary>
public class FileStorageService
{
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");

    /// <summary>
    /// Инициализирует новый экземпляр сервиса и создаёт папку хранилища, если она отсутствует.
    /// </summary>
    public FileStorageService()
    {
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
    }

    /// <summary>
    /// Асинхронное сохраняет загруженный файл на диск сервера.
    /// </summary>
    public async Task<string> SaveFileAsync(IFormFile file, string storedName)
    {
        var filePath = Path.Combine(_storagePath, storedName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return filePath;
    }

    /// <summary>
    /// Возвращает полный физический путь к файлу в хранилища.
    /// </summary>
    public string GetFilePath(string storedName)
    {
        return Path.Combine(_storagePath, storedName);
    }
}