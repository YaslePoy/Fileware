using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Fileware.Models;
using Fileware.Views;

namespace Fileware.Controls;

public partial class ImageBlock : IFileBlock
{
    public IMultiLevelView Host;

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

        if (DesiredSize.Height == BasePanel.DesiredSize.Height) return;
        Height = BasePanel.DesiredSize.Height;
    }

    public void AddTag(object? sender, RoutedEventArgs e)
    {
        AppContext.CurrentMultiLevelView.MakeTopLevel("TagManager", DataContext);
    }

    protected void OnRename(object? sender, RoutedEventArgs e)
    {
        Host.MakeTopLevel("FileRename", DataContext);
    }
}