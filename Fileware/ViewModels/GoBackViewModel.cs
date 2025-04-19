using System.Windows.Input;
using ReactiveUI;

namespace Fileware.ViewModels;

public class GoBackViewModel : ReactiveObject
{
    public IScreen Base { get; set; }
    public ICommand GoBack => ReactiveCommand.Create(() => Base.Router.NavigateBack.Execute());
}