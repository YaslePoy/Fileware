using ReactiveUI;

namespace Fileware.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public MainWindowViewModel()
    {
        Router.Navigate.Execute(new BasePageViewModel(this));
    }
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    public RoutingState Router { get; } = new();
}