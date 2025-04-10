using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Fileware.ViewModels;
using ReactiveUI;

namespace Fileware.Views;

public partial class Start : ReactiveUserControl<StartViewModel>
{
    public Start()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);

    }
}