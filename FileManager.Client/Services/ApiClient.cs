using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FileManager.Client.Models;

namespace FileManager.Client.Services;

/// <summary>
/// HTTP-клиент для взаимодействия с REST API сервера файлового менеджера.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5191/")
    };

    /// <summary>
    /// Запрашивает у сервера список всех сохранённых файлов.
    /// </summary>
    public async Task<List<FileItem>> GetFilesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<FileItem>>("api/files");
        return response ?? new List<FileItem>();
    }

    /// <summary>
    /// Загружает локальный файл с диска на сервер.
    /// </summary>
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

    /// <summary>
    /// Скачивает файл с сервера во временную папку и запускает его на клиенте.
    /// </summary>
    public async Task OpenFileLocallyAsync(FileItem fileItem)
    {
        var response = await _httpClient.GetAsync($"api/files/download/{fileItem.Id}");
        if (!response.IsSuccessStatusCode) return;
        
        var tempFolder = Path.Combine(Path.GetTempPath(), "FileManagerCache");
        if (!Directory.Exists(tempFolder))
            Directory.CreateDirectory(tempFolder);

        var localPath = Path.Combine(tempFolder, fileItem.OriginalName);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(localPath, bytes);
        
        Process.Start(new ProcessStartInfo
        {
            FileName = localPath,
            UseShellExecute = true
        });
    }
}