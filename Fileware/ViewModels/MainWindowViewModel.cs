using ReactiveUI;

namespace Fileware.ViewModels;

public partial class MainWindowViewModel : ReactiveObject, IScreen
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    public RoutingState Router { get; } = new RoutingState();
    
    public MainWindowViewModel()
    {
        Router.Navigate.Execute(new StartViewModel(this));
    }
}