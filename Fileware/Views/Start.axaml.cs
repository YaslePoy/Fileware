using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Fileware.ViewModels;
using ReactiveUI;

namespace Fileware.Views;

public partial class StartPage : ReactiveUserControl<StartPageViewModel>
{
    public StartPage()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}