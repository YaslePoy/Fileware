using System;
using System.Net.Http.Json;
using System.Windows.Input;
using ReactiveUI;

namespace Fileware.ViewModels;

public class RegisterPageViewModel : ReactiveObject, IRoutableViewModel
{
    private bool _loginActive;

    public RegisterPageViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
    }

    public ICommand Register => ReactiveCommand.Create(() =>
    {
        this.RaiseAndSetIfChanged(ref _loginActive, false, nameof(LoginActive));
        Api.Http.PostAsJsonAsync("api/User/reg",
            new
            {
                Username,
                ShowName,
                Password,
                BirthDate = DateOnly.Parse(BirthDate).ToString("yyyy-MM-dd")
            }).ContinueWith(_ =>
        {
            // var user = Api.Http.GetFromJsonAsync<GetUserResponse>($"api/User/{}");
        });
    });

    public bool LoginActive
    {
        get => _loginActive;
        set => _loginActive = value;
    }

    public string Username { get; set; }
    public string ShowName { get; set; }
    public string Password { get; set; }
    public string BirthDate { get; set; }
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
}