using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Fileware.Views;

namespace Fileware;

public static class AppContext
{
    public static MainWindow MainWindow;
    public static Dictionary<int, string> LocalStoredFiles;
    public const string StorageDir = "storage/";

    public static void SaveFileList()
    {
        File.WriteAllText("storedFiles.json", JsonSerializer.Serialize(LocalStoredFiles));
    }

    static AppContext()
    {
        if (File.Exists("storedFiles.json"))
            LocalStoredFiles =
                JsonSerializer.Deserialize<Dictionary<int, string>>(File.ReadAllText("storedFiles.json"));
        else
        {
            using var file = File.Create("storedFiles.json");
            LocalStoredFiles = new Dictionary<int, string>();
        }
    }
}