using Avalonia.Controls;
using Avalonia.Input;
using FileManager.Client.ViewModels;

namespace FileManager.Client.Views;

public partial class MainWindow : Window
{
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