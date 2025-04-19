using System;
using System.Reactive;
using ReactiveUI;

namespace Fileware.ViewModels;

public class StartPageViewModel : ReactiveObject, IRoutableViewModel
{
    public StartPageViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

    public ReactiveCommand<Unit, IRoutableViewModel> GoAuth => ReactiveCommand.CreateFromObservable(() =>
        HostScreen.Router.Navigate.Execute(new LoginPageViewModel(HostScreen)));

    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
}