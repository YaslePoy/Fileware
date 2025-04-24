using System;
using System.Windows.Input;
using ReactiveUI;

namespace Fileware.ViewModels;

public class TilesViewModel : ReactiveObject, IRoutableViewModel
{
    private bool _filterVisibility;

    public TilesViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
    }

    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
    public string StringFilter { get; set; }

    public bool FilterVisibility
    {
        get => _filterVisibility;
        set => this.RaiseAndSetIfChanged(ref _filterVisibility, value);
    }

    public ICommand FilteringSwitch => ReactiveCommand.Create(() =>
    {
        FilterVisibility = !FilterVisibility;
    });
}