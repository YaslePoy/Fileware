using System.IO;
using Avalonia.Input;
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
        vm.HostScreen.Router.Navigate.Execute(new EditProfileViewModel(vm.HostScreen, vm.User));
    }

    private void Exit(object? sender, TappedEventArgs e)
    {
        Directory.Delete("./UserData", true);
        AppContext.CurrentUser = null;
        (AppContext.WindowInstance.DataContext as MainWindowViewModel).Router.Navigate
            .Execute(new StartPageViewModel(AppContext.WindowInstance.DataContext as MainWindowViewModel));
    }
}