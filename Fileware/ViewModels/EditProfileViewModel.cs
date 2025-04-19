using System;
using Fileware.Models;
using ReactiveUI;

namespace Fileware.ViewModels;

public class EditProfileViewModel : ReactiveObject, IRoutableViewModel
{
    public EditProfileViewModel(IScreen hostScreen, UserData editingUser)
    {
        HostScreen = hostScreen;
        User = editingUser;
    }

    public EditProfileViewModel()
    {
    }

    public UserData User { get; set; }
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
}