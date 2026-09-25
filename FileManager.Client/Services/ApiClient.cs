using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FileManager.Client.Models;

namespace FileManager.Client.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5191/")
    };

    // 1. Получение списка всех файлов
    public async Task<List<FileItem>> GetFilesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<FileItem>>("api/files");
        return response ?? new List<FileItem>();
    }

    // 2. Загрузка файла на сервер
    public async Task<bool> UploadFileAsync(string filePath)
    {
        if (!File.Exists(filePath)) return false;

        using var content = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(filePath);
        using var streamContent = new StreamContent(fileStream);

        content.Add(streamContent, "file", Path.GetFileName(filePath));

        var response = await _httpClient.PostAsync("api/files/upload", content);
        return response.IsSuccessStatusCode;
    }

    // 3. Скачивание и открытие файла во внешнем приложении ОС
    public async Task OpenFileLocallyAsync(FileItem fileItem)
    {
        var response = await _httpClient.GetAsync($"api/files/download/{fileItem.Id}");
        if (!response.IsSuccessStatusCode) return;

        // Сохраняем во временную папку ОС
        var tempFolder = Path.Combine(Path.GetTempPath(), "FileManagerCache");
        if (!Directory.Exists(tempFolder))
            Directory.CreateDirectory(tempFolder);

        var localPath = Path.Combine(tempFolder, fileItem.OriginalName);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(localPath, bytes);

        // Запуск через стандартное приложение ОС
        Process.Start(new ProcessStartInfo
        {
            FileName = localPath,
            UseShellExecute = true
        });
    }
}