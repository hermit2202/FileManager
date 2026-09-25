using System;

namespace FileManager.Client.Models;

public class FileItem
{
    public Guid? Id { get; set; } // Заполняется для файлов с сервера
    public string OriginalName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty; // Заполняется для локальных файлов
    public bool IsDirectory { get; set; }
    public bool IsServerFile { get; set; } // Флаг: локальный файл или серверный
    public long Size { get; set; }
    public string FormattedSize { get; set; } = string.Empty;
    public DateTime DateUploaded { get; set; }
}