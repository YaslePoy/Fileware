using System;
using System.Windows.Input;
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

    public CommonUserData CommonUser { get; set; } = AppContext.CurrentUser.UserData;

    public ICommand GoManage =>
        ReactiveCommand.CreateFromObservable(() =>
            HostScreen.Router.Navigate.Execute(new TilesViewModel(HostScreen)));

    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
}