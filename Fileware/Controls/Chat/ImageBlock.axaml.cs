using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Fileware.Models;
using Fileware.Windows;

namespace Fileware.Controls;

public partial class ImageBlock : FileBlock
{
    public ImageBlock()
    {
        InitializeComponent();
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        var current = DataContext as FileData;
        Api.Http.DeleteAsync($"api/File/{current.Id}");
        AppContext.ChatPageInstance.PointsPanel.Children.Remove(this);
    }

    protected override void OnDataContextEndUpdate()
    {
        base.OnDataContextEndUpdate();
        var data = DataContext as FileData;
        data.TagsPreviewPanel = TagsPanel;
        data.UpdateTagPanel();
    }
    
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        Debug.Print("Changed size of image block");
    }
}