using System.Net.Http;

namespace Fileware;

public static class Api
{
    public const string ApiUrl = "http://217.66.19.16:8000/api/";
    public static HttpClient Http = new HttpClient();
}