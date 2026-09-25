using System;
using Avalonia;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace FileManager.Client;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI()
            .WithInterFont()
            .LogToTrace();
}