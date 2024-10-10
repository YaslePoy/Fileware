using System.Net.Http;
using System.Text.Json;

namespace Fileware;

public static class Api
{
    public const string ApiUrl = "http://217.66.19.16:8000/";
    public static HttpClient Http = new();

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}