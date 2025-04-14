using System;
using Fileware.Models;
using ReactiveUI;

namespace Fileware.ViewModels;

public class EditProfileViewModel : ReactiveObject, IRoutableViewModel
{
    public EditProfileViewModel(IScreen hostScreen, User editingUser)
    {
        HostScreen = hostScreen;
        User = editingUser;
    }

    public EditProfileViewModel()
    {
    }

    public User User { get; set; }
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
}