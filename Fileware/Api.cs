using System;
using System.Net.Http;
using System.Text.Json;

namespace Fileware;

public static class Api
{

#if LOCALHOST
    public const string ApiUrl = "http://localhost:8000/";
#else
    public const string ApiUrl = "http://95.105.78.72:8000/";
#endif

    // public const string ApiUrl = "http://192.168.1.43:8000/";
    public static HttpClient Http = new() {BaseAddress = new Uri(ApiUrl)};

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}