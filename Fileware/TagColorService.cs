using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Avalonia.Media;

namespace Fileware;

public static class TagColorService
{
    private static readonly Dictionary<string, Color>
        StartupColorPreferences = new() { { "Избранное", Colors.Orange } };

    public static Dictionary<string, Color> ColorPreferences = StartupColorPreferences;

    public static Color GetColorByString(string value)
    {
        if (ColorPreferences.TryGetValue(value, out var color)) return color;

        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(value));
        return new Color(byte.MaxValue, bytes[0], bytes[1], bytes[2]);
    }
}