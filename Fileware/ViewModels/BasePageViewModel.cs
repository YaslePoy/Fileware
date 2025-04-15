using System;
using ReactiveUI;

namespace Fileware.ViewModels;

public class BasePageViewModel : ReactiveObject, IRoutableViewModel, IScreen
{
    private double _dockPosition = 0.5;

    public BasePageViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
        Router.Navigate.Execute(new FileChatViewModel(this));
    }

    public double DockPosition
    {
        get => _dockPosition;
        set => this.RaiseAndSetIfChanged(ref _dockPosition, value);
    }

    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
    public RoutingState Router { get; } = new();
}