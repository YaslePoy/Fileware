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
}