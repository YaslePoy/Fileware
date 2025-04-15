using ReactiveUI;

namespace Fileware.ViewModels;

public class RenameViewModel : ReactiveObject
{
    public string FileName { get; set; }
}