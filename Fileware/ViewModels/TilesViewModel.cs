using System;
using ReactiveUI;

namespace Fileware.ViewModels;

public class TilesViewModel : ReactiveObject, IRoutableViewModel
{
    public TilesViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
    }

    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
}