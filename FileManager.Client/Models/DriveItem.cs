namespace FileManager.Client.Models;

/// <summary>
/// Модель данных локального диска.
/// </summary>
public class DriveItem
{
    /// <summary>
    /// Отображаемое имя диска.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Системный путь к корневой папке диска.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}