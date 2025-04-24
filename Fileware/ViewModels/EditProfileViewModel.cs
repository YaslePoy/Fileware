using System;
using ReactiveUI;

namespace Fileware.ViewModels;

public class EditProfileViewModel : ReactiveObject, IRoutableViewModel
{
    public EditProfileViewModel(IScreen hostScreen, CommonUserData editingCommonUser)
    {
        HostScreen = hostScreen;
        CommonUser = editingCommonUser;
    }

    public EditProfileViewModel()
    {
    }

    public CommonUserData CommonUser { get; set; }
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
}