using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using QRCoder;
using ReactiveUI;

namespace Fileware.ViewModels;

public class RegisterPageViewModel : ReactiveObject, IRoutableViewModel
{
    private bool _loginActive;
    private bool _useTotpAuth;
    private Bitmap? _totpBitmap;
    private double _imageSize;
    private double _spacing;
    private string _username;
    private string _showName;
    private string _password;
    private string _birthDate;
    private readonly string? _urlPathSegment = Guid.NewGuid().ToString()[..5];
    private readonly IScreen _hostScreen;
    private bool _showQr;
    private bool _passwordStage;
    private double _passwordOpacity;
    private string _alertText;
    private string _passAlertText;
    private bool _finalStage;
    private string _globalAlertText;

    public Bitmap? TotpBitmap
    {
        get => _totpBitmap;
        set => this.RaiseAndSetIfChanged(ref _totpBitmap, value);
    }

    public RegisterPageViewModel(IScreen hostScreen)
    {
        _hostScreen = hostScreen;
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
                TotpKey = _useTotpAuth ? new[] { 0 } : null,
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

    public string Username
    {
        get => _username;
        set => _username = value;
    }

    public string ShowName
    {
        get => _showName;
        set => _showName = value;
    }

    public string Password
    {
        get => _password;
        set => _password = value;
    }

    public string BirthDate
    {
        get => _birthDate;
        set => _birthDate = value;
    }

    public string? UrlPathSegment => _urlPathSegment;

    public IScreen HostScreen => _hostScreen;

    public bool UseTotpAuth
    {
        get => _useTotpAuth;

        set
        {
            _useTotpAuth = value;
            if (_useTotpAuth && TotpBitmap is null && !string.IsNullOrWhiteSpace(Username))
            {
                Api.Http.GetStringAsync("api/User/totp?username=" + HttpUtility.UrlEncode(Username)).ContinueWith(t =>
                {
                    Bitmap newTotp;
                    using (var qrGenerator = new QRCodeGenerator())
                    {
                        var qrCodeData = qrGenerator.CreateQrCode(t.Result, QRCodeGenerator.ECCLevel.Q);
                        using (var qrCode = new PngByteQRCode(qrCodeData))
                        {
                            using var ms = new MemoryStream(qrCode.GetGraphic(20));
                            newTotp = new Bitmap(ms);
                        }
                    }

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        TotpBitmap = newTotp;
                        ShowQr = true;

                        Debug.Print("Show image");
                    });
                });
            }
            else
            {
                ShowQr = value;
            }
        }
    }

    public bool ShowQr
    {
        get => _showQr;
        set => this.RaiseAndSetIfChanged(ref _showQr, value);
    }

    public bool PasswordStage
    {
        get => _passwordStage;
        set => this.RaiseAndSetIfChanged(ref _passwordStage, value);
    }

    public double PasswordOpacity
    {
        get => _passwordOpacity;
        set => this.RaiseAndSetIfChanged(ref _passwordOpacity, value);
    }

    public ICommand TryGotoPassword => ReactiveCommand.Create(() =>
    {
        Api.Http.GetStringAsync("api/User/exists?username=" + HttpUtility.UrlEncode(Username))
            .ContinueWith(task =>
            {
                if (bool.Parse(task.Result))
                {
                    AlertText = "Этот логин уже занят. Попробуйте другой";
                    return;
                }

                if (!DateOnly.TryParse(BirthDate, out var dateOnly))
                {
                    AlertText = "Правильно введите вашу дату рождения";
                    return;
                }

                if (string.IsNullOrWhiteSpace(ShowName))
                {
                    AlertText = "Заполните ваше имя";
                    return;
                }

                PasswordStage = true;
                PasswordOpacity = 1.0;
            });
    });

    public string AlertText
    {
        get => _alertText;
        set => this.RaiseAndSetIfChanged(ref _alertText, value);
    }

    public GoBackViewModel GoBackVM => new() { Base = HostScreen };

    public string PassAlertText
    {
        get => _passAlertText;
        set => this.RaiseAndSetIfChanged(ref _passAlertText, value);
    }

    public ICommand ConfirmPassword => ReactiveCommand.Create(() =>
    {
        if (Password is null or { Length: < 6})
        {
            PassAlertText = "Пароль не может быть меньше 6 символов";
            return;
        }

        if (UseTotpAuth)
        {
            if (!bool.Parse(Api.Http.GetStringAsync($"api/User/totp/{Username}/verify?totpKey=" + Password).GetAwaiter()
                    .GetResult()))
            {
                PassAlertText = "Введенный пароль устарел. Попробуйте снова";
                return;
            }
        }


        FinalStage = true;
    });

    public bool FinalStage
    {
        get => _finalStage;
        set => this.RaiseAndSetIfChanged(ref _finalStage, value);
    }

    public ICommand RegisterFinal => ReactiveCommand.Create(() =>
    {
        var req = JsonSerializer.Serialize(new
        {
            Username,
            ShowName,
            Password,
            TotpKey = _useTotpAuth ?  Convert.ToBase64String(Encoding.Default.GetBytes(Password)) : null,
            BirthDate = DateOnly.Parse(BirthDate).ToString("yyyy-MM-dd")
        }, Api.JsonOptions);
        Api.Http.PostAsync("api/User/reg", new StringContent(req, Api.JsonMediaType)).ContinueWith(t =>
        {
            if (!t.Result.IsSuccessStatusCode)
            {
                GlobalAlertText = "Упс. Что-жто пошло не так";
                return;
            }

            Dispatcher.UIThread.Invoke(() =>
            {
                HostScreen.Router.Navigate.Execute(new LoginPageViewModel(HostScreen));
            });
        });
    });

    public string GlobalAlertText
    {
        get => _globalAlertText;
        set => this.RaiseAndSetIfChanged(ref _globalAlertText, value);
    }
}