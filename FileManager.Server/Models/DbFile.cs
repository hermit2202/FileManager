namespace FileManager.Server.Models;

public class DbFile
{
    public Guid Id { get; set; }
    public string OriginalName { get; set; } = String.Empty;
    public string StoredName { get; set; } = String.Empty;
    public long Size { get; set; }
    public DateTime DateUploaded { get; set; } = DateTime.UtcNow;
}