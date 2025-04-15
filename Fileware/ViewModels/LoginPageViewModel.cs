using System;
using System.Diagnostics;
using System.Windows.Input;
using ReactiveUI;

namespace Fileware.ViewModels;

public class LoginPageViewModel : ReactiveObject, IRoutableViewModel
{
    public LoginPageViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
    }

    public string Login { get; set; }
    public string Password { get; set; }

    public ICommand GoRegister => ReactiveCommand.CreateFromObservable(() =>
        HostScreen.Router.Navigate.Execute(new RegisterPageViewModel(HostScreen)));

    public ICommand TryAuth => ReactiveCommand.Create(() =>
    {
        Debug.WriteLine($"Login: {Login}, password: {Password}");
        HostScreen.Router.Navigate.Execute(new BasePageViewModel(HostScreen));
    });

    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
}