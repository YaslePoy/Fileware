using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
       var auth = await Http.PostAsync($"api/User/auth?login={HttpUtility.UrlEncode(login)}&password={HttpUtility.UrlEncode(password)}", new StringContent(""));
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
}

public class UserData
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string ShowName { get; set; }
    public DateOnly BirthDate { get; set; }
    public int FileCount { get; set; }
    public List<string> AttachedFilespaces { get; set; }
    [JsonIgnore]
    public Task<Bitmap?> AvatarImage { get; } =
        ImageHelper.LoadFromWeb(new Uri("https://upload.wikimedia.org/wikipedia/commons/4/41/NewtonsPrincipia.jpg"));

}