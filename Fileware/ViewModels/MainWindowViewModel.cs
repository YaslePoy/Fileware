using System.IO;
using System.Text.Json;
using ReactiveUI;

namespace Fileware.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public MainWindowViewModel()
    {
        if (!File.Exists("./UserData/user.json"))
        {
            Router.Navigate.Execute(new StartPageViewModel(this));
        }
        else
        {
            AppContext.CurrentUser =
                JsonSerializer.Deserialize<LoginResponse>(File.ReadAllText("./UserData/user.json"), Api.JsonOptions);

            Api.Http.GetStringAsync($"api/User/{AppContext.CurrentUser.UserData.Id}").ContinueWith(t =>
            {
                var data = JsonSerializer.Deserialize<UserData>(t.Result, Api.JsonOptions);
                Utils.TransferData(AppContext.CurrentUser.UserData, data);
                AppContext.CurrentUser.Save(api: false);
            });

            Router.Navigate.Execute(
                new BasePageViewModel(this));
        }
    }
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    public RoutingState Router { get; } = new();
}