using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Fileware.ViewModels;
using ReactiveUI;

namespace Fileware.Views;

public partial class LoginPage : ReactiveUserControl<LoginPageViewModel>
{
    public LoginPage()
    {
        this.WhenActivated(_ => { });
        AvaloniaXamlLoader.Load(this);    
    }
}