using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Fileware.Windows;

public partial class FileRenameWindow : Window
{
    public FileRenameWindow()
    {
        InitializeComponent();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void OnApply(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
}