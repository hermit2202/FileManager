using FileManager.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Server.Data;

/// <summary>
/// Контекст базы данных.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Инициализирует новый экземпляр контекста с заданными переметрами подключения.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    /// <summary>
    /// Набор файлов, сохранённых в системе.
    /// </summary>
    public DbSet<DbFile> Files => Set<DbFile>();
}