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

    private void OnRename(object? sender, RoutedEventArgs e)
    {
        var win = new FileRenameWindow { DataContext = DataContext };
        win.ShowDialog<bool>(AppContext.WindowInstance).ContinueWith(t =>
        {
            if (t.Result)
                Dispatcher.UIThread.Invoke(() =>
                {
                    var msg = DataContext as FileData;
                    msg.OnPropertyChanged("Name");
                    Api.Http.PatchAsync($"api/File/{msg.Id}/rename",
                        new StringContent("\"" + msg.Name + "\"",
                            MediaTypeWithQualityHeaderValue.Parse("application/json")));
                });
        });
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        var current = DataContext as FileData;
        Api.Http.DeleteAsync($"api/File/{current.Id}");
        AppContext.ChatInstance.PointsPanel.Children.Remove(this);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        Debug.Print("Changed size of image block");
    }
}