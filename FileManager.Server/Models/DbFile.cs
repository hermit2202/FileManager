namespace FileManager.Server.Models;

/// <summary>
/// Модель данных файла.
/// </summary>
public class DbFile
{
    /// <summary>
    /// Уникальный идентификатор файла.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Оригинальное имя файла.
    /// </summary>
    public string OriginalName { get; set; } = String.Empty;
    
    /// <summary>
    /// Изменённое имя файла.
    /// </summary>
    public string StoredName { get; set; } = String.Empty;
    
    /// <summary>
    /// Размер файла.
    /// </summary>
    public long Size { get; set; }
    
    /// <summary>
    /// Время изменения файла.
    /// </summary>
    public DateTime DateUploaded { get; set; } = DateTime.UtcNow;
}