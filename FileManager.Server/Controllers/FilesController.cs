using System;
using System.IO;
using System.Threading.Tasks;
using FileManager.Server.Data;
using FileManager.Server.Models;
using FileManager.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly FileStorageService _storage;

    public FilesController(AppDbContext db, FileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFiles()
    {
        var files = await _db.Files.ToListAsync();
        return Ok(files);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не выбран");

        var ext = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid()}{ext}";

        await _storage.SaveFileAsync(file, storedName);

        var dbFile = new DbFile
        {
            OriginalName = file.FileName,
            StoredName = storedName,
            Size = file.Length
        };

        _db.Files.Add(dbFile);
        await _db.SaveChangesAsync();

        return Ok(dbFile);
    }

    [HttpGet("download/{id:guid}")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        var dbFile = await _db.Files.FindAsync(id);
        if (dbFile == null) return NotFound("Файл не найден в базе данных");

        var path = _storage.GetFilePath(dbFile.StoredName);
        if (!System.IO.File.Exists(path)) return NotFound("Файл не найден на диске");

        var bytes = await System.IO.File.ReadAllBytesAsync(path);
        return File(bytes, "application/octet-stream", dbFile.OriginalName);
    }
}