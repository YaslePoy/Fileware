using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Fileware.ViewModels;
using ReactiveUI;

namespace Fileware.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
        AppContext.WindowInstance = this;
    }
}