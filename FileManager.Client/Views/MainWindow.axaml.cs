using Avalonia.Controls;
using Avalonia.Input;
using FileManager.Client.ViewModels;

namespace FileManager.Client.Views;

/// <summary>
/// Главное окно пользовательского интерфейса приложения.
/// Связывает XAML-разметку представление (View) с логикой ViewModel и обрабатывает события ввода.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MainWindow"/>
    /// и связывает контекст данных (DataContext) с <see cref="MainViewModel"/>.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private async void OnListBoxDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            await vm.OpenSelectedAsync();
        }
    }
}