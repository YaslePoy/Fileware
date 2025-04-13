using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Fileware.ViewModels;

namespace Fileware.Views;

public partial class BasePage : ReactiveUserControl<BasePageViewModel>
{
    public BasePage()
    {
        InitializeComponent();
    }

    private void OpenDock(object? sender, PointerEventArgs e)
    {
        Debug.Print("Opening dock");
        DockBorder.Height = 50;
    }

    private void CloseDock(object? sender, PointerEventArgs e)
    {
        Debug.Print("Closing dock");
        DockBorder.Height = 5;
    }

    private void UpdateGradient(object? sender, PointerEventArgs e)
    {
        var dock = sender as Border;
        var position = e.GetPosition(dock);
        var progress = position.X / dock.DesiredSize.Width;
        (DataContext as BasePageViewModel).DockPosition = progress;
        Debug.Print(progress.ToString());
    }
}