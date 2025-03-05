using System.Net.Http;
using System.Text.Json;

namespace Fileware;

public static class Api
{
    public const string ApiUrl = "http://95.105.78.72:8000/";

    // public const string ApiUrl = "http://192.168.1.43:8000/";
    public static HttpClient Http = new();

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}