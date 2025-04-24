using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Avalonia.Media;

namespace Fileware;

public static class TagColorService
{
    static TagColorService()
    {
        Debug.Print("Creating TagColorService");
        if (string.IsNullOrWhiteSpace(AppContext.CurrentUser.UserData.Preferences))
        {
            Save();
        }
        else
        {
            ColorPreferences = JsonSerializer.Deserialize<Dictionary<string, string>>(AppContext.CurrentUser.UserData.Preferences).ToDictionary(i => i.Key, i => Color.Parse(i.Value));
        }
    }
    
    private static readonly Dictionary<string, Color>
        StartupColorPreferences = new() { { "Избранное", Colors.Orange } };

    public static Dictionary<string, Color> ColorPreferences = StartupColorPreferences;

    public static Color GetColorByString(string value)
    {
        if (ColorPreferences.TryGetValue(value, out var color)) return color;

        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(value));
        return new Color(byte.MaxValue, bytes[0], bytes[1], bytes[2]);
    }

    public static void Save()
    {
        AppContext.CurrentUser.UserData.Preferences = JsonSerializer.Serialize(ColorPreferences.ToDictionary(i => i.Key, i => i.Value.ToString()));
        AppContext.CurrentUser.Save();
    }
}