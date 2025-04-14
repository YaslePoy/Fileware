using System;
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

    public User User { get; set; }
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
}