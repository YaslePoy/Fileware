using System;
using ReactiveUI;

namespace Fileware.ViewModels;

public class FileChatViewModel : ReactiveObject, IRoutableViewModel
{
    public FileChatViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
    }

    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
}