using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
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
            new StringContent(JsonSerializer.Serialize(tags.Select(i => i.Name)), MediaTypeWithQualityHeaderValue.Parse("application/json")));
    }
}