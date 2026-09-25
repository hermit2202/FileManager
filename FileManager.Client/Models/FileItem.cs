using System;

namespace FileManager.Client.Models;

/// <summary>
/// Модель данных файла или дириктории.
/// </summary>
public class FileItem
{
    /// <summary>
    /// Уникальный идентификатор файла или дириктории.
    /// </summary>
    public Guid? Id { get; set; } 
    
    /// <summary>
    /// Оригинальное имя файла или дириктории.
    /// </summary>
    public string OriginalName { get; set; } = string.Empty;
    
    /// <summary>
    /// Полный путь к файлу или дириктории.
    /// </summary>
    public string FullPath { get; set; } = string.Empty; 
    
    /// <summary>
    /// Флаг, является ли объект директорией.
    /// </summary>
    public bool IsDirectory { get; set; }
    
    /// <summary>
    /// Флаг, расположен ли файл в сетевом хранилище на сервере.
    /// </summary>
    public bool IsServerFile { get; set; } 
    
    /// <summary>
    /// Размер файла в байтах.
    /// </summary>
    public long Size { get; set; }
    
    /// <summary>
    /// Фарматированный размер файла для отображения в UI.
    /// </summary>
    public string FormattedSize { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата и времяя последней модификации локального диска или загрузки на сервер.
    /// </summary>
    public DateTime DateUploaded { get; set; }
}