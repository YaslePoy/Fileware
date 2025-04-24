using System;
using System.IO;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Fileware.ViewModels;
using ReactiveUI;

namespace Fileware.Views;

public partial class ProfilePage : ReactiveUserControl<ProfileViewModel>
{
    public ProfilePage()
    {
        this.WhenActivated(_ => { });
        InitializeComponent();
    }

    private void EditProfile(object? sender, TappedEventArgs e)
    {
        var vm = DataContext as ProfileViewModel;
        vm.HostScreen.Router.Navigate.Execute(new EditProfileViewModel(vm.HostScreen, vm.CommonUser));
    }

    private void Exit(object? sender, TappedEventArgs e)
    {
        Directory.Delete("./UserData", true);
        AppContext.CurrentUser = null;
        (AppContext.WindowInstance.DataContext as MainWindowViewModel).Router.Navigate
            .Execute(new StartPageViewModel(AppContext.WindowInstance.DataContext as MainWindowViewModel));
    }

    private void EditName(object? sender, RoutedEventArgs e)
    {
        var dc = DataContext as ProfileViewModel;
        RenamingPanel.DataContext = new RenameViewModel { AnyName = dc.CommonUser.ShowName };
        RenamingPanel.IsVisible = true;
    }

    private void EditDate(object? sender, RoutedEventArgs e)
    {
        var dc = DataContext as ProfileViewModel;
        RedatePanel.DataContext = new RenameViewModel { AnyName = dc.CommonUser.BirthDate.ToString("MM.dd.yyyy") };
        RedatePanel.IsVisible = true;
    }

    private void EditAvatar(object? sender, RoutedEventArgs e)
    {
        
    }

    private void OnCancelRename(object? sender, RoutedEventArgs e)
    {
        RenamingPanel.IsVisible = false;
        RedatePanel.IsVisible = false;
    }

    private void OnApplyRename(object? sender, RoutedEventArgs e)
    {
        if (RenamingPanel.IsVisible)
        {
            var vm = RenamingPanel.DataContext as RenameViewModel;

            var curvm = DataContext as ProfileViewModel;
            curvm.CommonUser.ShowName = vm.AnyName;
            AppContext.CurrentUser.Save();
            RenamingPanel.IsVisible = false;    
        }else if (RedatePanel.IsVisible)
        {
            var vm = RedatePanel.DataContext as RenameViewModel;

            var curvm = DataContext as ProfileViewModel;
            if (!DateOnly.TryParse(vm.AnyName, out var dateOnly))
            {
                vm.AlertText = "Введите корректную дату";
                return;
            }
            curvm.CommonUser.BirthDate = dateOnly;
            AppContext.CurrentUser.Save();
            RedatePanel.IsVisible = false; 
        }
        
    }
}