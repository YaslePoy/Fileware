using System;
using ReactiveUI;

namespace Fileware.ViewModels;

public class FileSpaceViewModel : ReactiveObject, IRoutableViewModel
{
    public FileSpaceViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
        GoBackVM = new GoBackViewModel { Base = HostScreen };
    }

    public GoBackViewModel GoBackVM { get; } 

    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; set; }
}