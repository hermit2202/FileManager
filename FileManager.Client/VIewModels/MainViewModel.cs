using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using FileManager.Client.Models;
using FileManager.Client.Services;
using ReactiveUI;

namespace FileManager.Client.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly ApiClient _apiClient = new();

    private bool _isServerFolder;
    private string _currentPath = string.Empty;
    private string _searchPattern = string.Empty;
    private FileItem? _selectedFile;
    private string _statusMessage = string.Empty;

    public ObservableCollection<DriveItem> Drives { get; } = new();
    public ObservableCollection<FileItem> AllItems { get; } = new();
    public ObservableCollection<FileItem> FilteredFiles { get; } = new();

    // Команды для XAML
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Task> UploadCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenServerFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenDesktopCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenDownloadsCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenDocumentsCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenPicturesCommand { get; }

    public bool IsServerFolder
    {
        get => _isServerFolder;
        set => this.RaiseAndSetIfChanged(ref _isServerFolder, value);
    }

    public string CurrentPath
    {
        get => _currentPath;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentPath, value);
            if (!IsServerFolder && !string.IsNullOrWhiteSpace(value))
            {
                LoadDirectoryContent(value);
            }
        }
    }

    public string SearchPattern
    {
        get => _searchPattern;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchPattern, value);
            ApplyFilter();
        }
    }

    public FileItem? SelectedFile
    {
        get => _selectedFile;
        set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public MainViewModel()
    {
        // Загрузка динамического списка дисков
        LoadDrives();

        // Инициализация команд
        RefreshCommand = ReactiveCommand.Create(() =>
        {
            if (IsServerFolder)
            {
                _ = OpenServerFolderAsync();
            }
            else
            {
                LoadDirectoryContent(CurrentPath);
            }
        });

        UploadCommand = ReactiveCommand.Create(async () => await UploadSelectedToFolderAsync());
        OpenServerFolderCommand = ReactiveCommand.Create(() => { _ = OpenServerFolderAsync(); });

        // Быстрый доступ с гарантированным сбросом флага IsServerFolder
        OpenDesktopCommand = ReactiveCommand.Create(() => { SetFolder(Environment.SpecialFolder.Desktop); });
        OpenDownloadsCommand = ReactiveCommand.Create(() => 
        { 
            IsServerFolder = false;
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            CurrentPath = Path.Combine(userPath, "Downloads");
        });
        OpenDocumentsCommand = ReactiveCommand.Create(() => { SetFolder(Environment.SpecialFolder.MyDocuments); });
        OpenPicturesCommand = ReactiveCommand.Create(() => { SetFolder(Environment.SpecialFolder.MyPictures); });

        // Начальная папка при запуске
        CurrentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    public void LoadDrives()
    {
        Drives.Clear();
        try
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    string label = string.IsNullOrWhiteSpace(drive.VolumeLabel) 
                        ? "Локальный диск" 
                        : drive.VolumeLabel;

                    Drives.Add(new DriveItem
                    {
                        Name = $"💾 {label} ({drive.Name.TrimEnd('\\')})",
                        Path = drive.Name
                    });
                }
            }
        }
        catch { }
    }

    public void OpenDrive(string drivePath)
    {
        IsServerFolder = false;
        CurrentPath = drivePath;
    }

    public void LoadDirectoryContent(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            StatusMessage = $"Папка не найдена: {path}";
            return;
        }

        Task.Run(() =>
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                var tempItems = new List<FileItem>();

                // Чтение каталогов
                try
                {
                    foreach (var dir in dirInfo.GetDirectories())
                    {
                        try
                        {
                            if (dir.Attributes.HasFlag(FileAttributes.Hidden) || 
                                dir.Attributes.HasFlag(FileAttributes.System)) 
                                continue;

                            tempItems.Add(new FileItem
                            {
                                OriginalName = $"📁 {dir.Name}",
                                FullPath = dir.FullName,
                                IsDirectory = true,
                                IsServerFile = false,
                                FormattedSize = "<Папка>",
                                DateUploaded = dir.LastWriteTime
                            });
                        }
                        catch { }
                    }
                }
                catch { }

                // Чтение файлов
                try
                {
                    foreach (var file in dirInfo.GetFiles())
                    {
                        try
                        {
                            if (file.Attributes.HasFlag(FileAttributes.Hidden)) continue;

                            tempItems.Add(new FileItem
                            {
                                OriginalName = $"📄 {file.Name}",
                                FullPath = file.FullName,
                                IsDirectory = false,
                                IsServerFile = false,
                                Size = file.Length,
                                FormattedSize = FormatSize(file.Length),
                                DateUploaded = file.LastWriteTime
                            });
                        }
                        catch { }
                    }
                }
                catch { }

                // Безопасный вывод в UI-поток
                Dispatcher.UIThread.Post(() =>
                {
                    IsServerFolder = false;
                    AllItems.Clear();
                    foreach (var item in tempItems) AllItems.Add(item);

                    ApplyFilter();
                    StatusMessage = $"Локальная папка: {path} | Элементов: {FilteredFiles.Count}";
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    StatusMessage = $"Ошибка открытия папки: {ex.Message}";
                });
            }
        });
    }

    public async Task OpenServerFolderAsync()
    {
        try
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsServerFolder = true;
                _currentPath = "☁ Сетевое хранилище (ASP.NET Core Server)";
                this.RaisePropertyChanged(nameof(CurrentPath));
                StatusMessage = "Загрузка файлов с сервера...";
                AllItems.Clear();
            });

            var serverFiles = await _apiClient.GetFilesAsync();

            Dispatcher.UIThread.Post(() =>
            {
                AllItems.Clear();
                foreach (var file in serverFiles)
                {
                    AllItems.Add(new FileItem
                    {
                        Id = file.Id,
                        OriginalName = $"☁ {file.OriginalName}",
                        FullPath = file.OriginalName,
                        IsDirectory = false,
                        IsServerFile = true,
                        Size = file.Size,
                        FormattedSize = file.FormattedSize,
                        DateUploaded = file.DateUploaded
                    });
                }

                ApplyFilter();
                StatusMessage = $"Сетевая папка | Файлов на сервере: {FilteredFiles.Count}";
            });
        }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Post(() =>
            {
                StatusMessage = $"Ошибка сервера: {ex.Message}";
            });
        }
    }

    public async Task UploadSelectedToFolderAsync()
    {
        if (SelectedFile == null || SelectedFile.IsDirectory || SelectedFile.IsServerFile)
        {
            StatusMessage = "Выберите локальный файл для отправки на сервер";
            return;
        }

        try
        {
            StatusMessage = $"Загрузка {SelectedFile.OriginalName} на сервер...";
            bool success = await _apiClient.UploadFileAsync(SelectedFile.FullPath);
            
            if (success)
            {
                StatusMessage = $"Файл {SelectedFile.OriginalName} успешно отправлен!";
            }
            else
            {
                StatusMessage = "Ошибка при передаче файла на сервер";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    public void ApplyFilter()
    {
        Dispatcher.UIThread.Post(() =>
        {
            FilteredFiles.Clear();
            var query = AllItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchPattern))
            {
                string pattern = SearchPattern.Trim().ToLower();
                query = query.Where(item =>
                {
                    string name = item.OriginalName.ToLower();
                    if (pattern.StartsWith("*"))
                        return name.EndsWith(pattern.TrimStart('*'));
                    return name.Contains(pattern);
                });
            }

            foreach (var item in query)
            {
                FilteredFiles.Add(item);
            }
        });
    }

    public void NavigateUp()
    {
        // Переход из сетевого хранилища обратно на ПК
        if (IsServerFolder)
        {
            IsServerFolder = false;
            CurrentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentPath)) return;

        var parentDir = Directory.GetParent(CurrentPath);
        if (parentDir != null)
        {
            CurrentPath = parentDir.FullName;
        }
    }

    public async Task OpenSelectedAsync()
    {
        if (SelectedFile == null) return;

        if (!SelectedFile.IsServerFile)
        {
            if (SelectedFile.IsDirectory)
            {
                CurrentPath = SelectedFile.FullPath;
            }
            else
            {
                Process.Start(new ProcessStartInfo 
                { 
                    FileName = SelectedFile.FullPath, 
                    UseShellExecute = true 
                });
            }
        }
        else
        {
            try
            {
                StatusMessage = "Загрузка файла с сервера...";
                await _apiClient.OpenFileLocallyAsync(SelectedFile);
                StatusMessage = "Файл открыт!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }
    }

    private void SetFolder(Environment.SpecialFolder folder)
    {
        IsServerFolder = false;
        CurrentPath = Environment.GetFolderPath(folder);
    }

    private static string FormatSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}