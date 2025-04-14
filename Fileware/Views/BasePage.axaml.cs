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

    private string _currentPage = "chat";

    private void OpenDock(object? sender, PointerEventArgs e)
    {
        Debug.Print("Opening dock");
        DockBorder.Height = 60;
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

    private void NavigateTo(object? sender, TappedEventArgs e)
    {
        if ((sender as Control).Tag == _currentPage)
            return;

        _currentPage = (sender as Control).Tag as string;

        var vm = DataContext as BasePageViewModel;

        switch (_currentPage)
        {
            case "chat":
                vm.Router.Navigate.Execute(new FileChatViewModel(vm));
                break;
            case "tile":
                vm.Router.Navigate.Execute(new TilesViewModel(vm));
                break;
            case "profile":
                vm.Router.Navigate.Execute(new ProfileViewModel(vm));
                break;
        }
    }
}