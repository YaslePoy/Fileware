using ReactiveUI;

namespace Fileware.ViewModels;

public class RenameViewModel : ReactiveObject
{
    private string _alertText;
    public string AnyName { get; set; }

    public string AlertText
    {
        get => _alertText;
        set => this.RaiseAndSetIfChanged(ref _alertText, value);
    }
}