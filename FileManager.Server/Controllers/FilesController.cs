using FileManager.Server.Data;
using FileManager.Server.Models;
using FileManager.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Server.Controllers;

/// <summary>
/// Контроллер для управления файлами.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly FileStorageService _storage;

    /// <summary>
    /// Инициализирует новый экземпляр контроллера с необходимыми сервисами.
    /// </summary>
    public FilesController(AppDbContext db, FileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    /// <summary>
    /// Получает список всех файлов, зарегистрированных в системе.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllFiles()
    {
        var files = await _db.Files.ToListAsync();
        return Ok(files);
    }

    /// <summary>
    /// Загружает файл на сервер и сохраняет его метаданные в базу данных.
    /// </summary>
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

    /// <summary>
    /// Скачивает файл по его уникальному идентификатору.
    /// </summary>
    [HttpGet("download/{id:guid}")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        var dbFile = await _db.Files.FindAsync(id);
        if (dbFile == null) return NotFound("Файл не найден в базе данных");

        var path = _storage.GetFilePath(dbFile.StoredName);
        if (!System.IO.File.Exists(path)) return NotFound("Файл не найден на диске");

        return PhysicalFile(path, "application/octet-stream", dbFile.OriginalName);
    }

    /// <summary>
    /// Удаляет файл из базы данных и физического хранилища по его уникальному идентификатору.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        var dbFile = await _db.Files.FindAsync(id);
        if (dbFile == null)
        {
            return NotFound("Файл не найден в базе данных");
        }

        bool isDeletedFromStorage = _storage.DeleteFile(dbFile.StoredName);
        if (!isDeletedFromStorage)
        {
            return StatusCode(500, "Не удалось удалить файл с диска. Проверьте права доступа к сетевому хранилищу или не открыт ли файл в другой программе.");
        }

        _db.Files.Remove(dbFile);
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Файл '{dbFile.OriginalName}' успешно удалён" });
    }
}