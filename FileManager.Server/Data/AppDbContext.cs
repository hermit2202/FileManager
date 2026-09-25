using FileManager.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<DbFile> Files => Set<DbFile>();
}