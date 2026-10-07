namespace FileManager.Server.Services;

/// <summary>
/// Сервис для физического сохранения, чтения и удаления файлов на диске сервера.
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
    /// Асинхронно сохраняет загруженный файл на диск сервера.
    /// </summary>
    public async Task<string> SaveFileAsync(IFormFile file, string storedName)
    {
        var filePath = Path.Combine(_storagePath, storedName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return filePath;
    }

    /// <summary>
    /// Возвращает полный физический путь к файлу в хранилище.
    /// </summary>
    public string GetFilePath(string storedName)
    {
        return Path.Combine(_storagePath, storedName);
    }

    /// <summary>
    /// Удаляет файл из физического хранилища на диске, если он существует.
    /// </summary>
    /// <param name="storedName">Уникальное имя сохранённого файла.</param>
    /// <returns>True, если файл успешно удалён или отсутствовал; иначе False.</returns>
    public bool DeleteFile(string storedName)
    {
        try
        {
            var filePath = GetFilePath(storedName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}