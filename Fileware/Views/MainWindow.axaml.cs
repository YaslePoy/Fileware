using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Fileware.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragEnterEvent, DragOver);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, DragDropEvent);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        Viewer.Effect = new ImmutableBlurEffect(15);
        LoadPanel.IsVisible = true;
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Viewer.Effect = null;
        LoadPanel.IsVisible = false;
    }

    private void DragDropEvent(object? sender, DragEventArgs e)
    {
        Viewer.Effect = null;
        LoadPanel.IsVisible = false;
    }
}