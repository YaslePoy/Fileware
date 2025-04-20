using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Media.Imaging;
using Fileware.Models;
using Fileware.ViewModels;

namespace Fileware;

public static class Api
{
#if LOCALHOST
    public const string ApiUrl = "http://localhost:8000/";
#else
    public const string ApiUrl = "http://95.105.78.72:8000/";
#endif

    // public const string ApiUrl = "http://192.168.1.43:8000/";
    public static HttpClient Http = new() { BaseAddress = new Uri(ApiUrl) };

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static Task UpdateTags(int point, List<Tag> tags)
    {
        return Http.PatchAsync($"api/History?pointId={point}",
            new StringContent(JsonSerializer.Serialize(tags.Select(i => i.Name)),
                MediaTypeWithQualityHeaderValue.Parse("application/json")));
    }

    public static async Task<LoginResponse?> Auth(string login, string password)
    {
        var auth = await Http.PostAsync(
            $"api/User/auth?login={HttpUtility.UrlEncode(login)}&password={HttpUtility.UrlEncode(password)}",
            new StringContent(""));
        if (auth.IsSuccessStatusCode)
        {
            return JsonSerializer.Deserialize<LoginResponse>(await auth.Content.ReadAsStringAsync(), JsonOptions);
        }

        return null;
    }
}

public class LoginResponse
{
    public string Token { get; set; }
    public UserData UserData { get; set; }

    public void Save(string path = "./UserData/user.json", bool api = true)
    {
        if (!Directory.Exists("./UserData"))
        {
            Directory.CreateDirectory("./UserData");
        }

        var saving = JsonSerializer.Serialize(this, Api.JsonOptions);

        if (api)
            SaveApi();

        File.WriteAllTextAsync(path, saving);
    }

    public Task SaveApi()
    {
        Api.Http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Bearer " + Token);
        return Api.Http.PatchAsync("api/User",
            new StringContent(JsonSerializer.Serialize(UserData, Api.JsonOptions), JsonMediaType));
    }

    public static MediaTypeWithQualityHeaderValue JsonMediaType =>
        MediaTypeWithQualityHeaderValue.Parse("application/json");
}

public class UserData : INotifyPropertyChanged
{
    private List<string> _attachedFileSpaces;
    private int _fileCount;
    private DateOnly _birthDate;
    private string _showName;
    public int Id { get; set; }
    public string Username { get; set; }

    public string ShowName
    {
        get => _showName;
        set
        {
            if (value == _showName) return;
            _showName = value;
            OnPropertyChanged();
        }
    }

    public DateOnly BirthDate
    {
        get => _birthDate;
        set
        {
            if (value.Equals(_birthDate)) return;
            _birthDate = value;
            OnPropertyChanged();
            OnPropertyChanged();
        }
    }

    public int FileCount
    {
        get => _fileCount;
        set
        {
            if (value == _fileCount) return;
            _fileCount = value;
            OnPropertyChanged();
            OnPropertyChanged();
        }
    }

    public List<string> AttachedFileSpaces
    {
        get => _attachedFileSpaces;
        set
        {
            if (Equals(value, _attachedFileSpaces)) return;
            _attachedFileSpaces = value;
            OnPropertyChanged();
        }
    }

    [JsonIgnore]
    public Task<Bitmap?> AvatarImage { get; } =
        ImageHelper.LoadFromWeb(new Uri("https://upload.wikimedia.org/wikipedia/commons/4/41/NewtonsPrincipia.jpg"));

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}