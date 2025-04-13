using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Fileware.Models;

namespace Fileware.Controls;

public partial class ImageBlock : FileBlock
{
    public ImageBlock()
    {
        InitializeComponent();
    }

    private void UploadNewer(object? sender, TappedEventArgs e)
    {
        
    }

    private void DownloadNewer(object? sender, TappedEventArgs e)
    {
        
    }

    private void OnRename(object? sender, RoutedEventArgs e)
    {
        
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        var current = DataContext as FileData;
        Api.Http.DeleteAsync($"api/File/{current.Id}");
        AppContext.ChatInstance.PointsPanel.Children.Remove(this);
    }
}