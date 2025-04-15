using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Fileware.ViewModels;
using ReactiveUI;

namespace Fileware.Views;

public partial class LoginPage : ReactiveUserControl<LoginPageViewModel>
{
    private const double Button_VISIBLE = 39;

    public LoginPage()
    {
        this.WhenActivated(_ => { });
        InitializeComponent();
    }

    private void UpdateLoginVisibility(object? sender, TextChangedEventArgs e)
    {
        LoginButton.Height = string.IsNullOrWhiteSpace(LoginText.Text) ||
                             string.IsNullOrWhiteSpace(PasswordText.Text) || PasswordText.Text.Length < 6
            ? 0
            : Button_VISIBLE;
    }
}