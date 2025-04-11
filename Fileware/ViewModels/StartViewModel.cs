using System;
using System.Reactive;
using ReactiveUI;

namespace Fileware.ViewModels;

public class StartViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }

    public ReactiveCommand<Unit, IRoutableViewModel> GoAuth => ReactiveCommand.CreateFromObservable(() =>
        HostScreen.Router.Navigate.Execute(new LoginPageViewModel(HostScreen)));

    public StartViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}