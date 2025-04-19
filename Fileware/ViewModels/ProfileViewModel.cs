using System;
using System.Reactive;
using Fileware.Models;
using ReactiveUI;

namespace Fileware.ViewModels;

public class ProfileViewModel : ReactiveObject, IRoutableViewModel
{
    public ProfileViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
    }

    public ProfileViewModel()
    {
    }

    public UserData User { get; set; } = AppContext.CurrentUser.UserData;
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }

    public IReactiveCommand<Unit, IRoutableViewModel> GoManage =>
        ReactiveCommand.CreateFromObservable(() => 
            HostScreen.Router.Navigate.Execute(new TilesViewModel(HostScreen)));
}