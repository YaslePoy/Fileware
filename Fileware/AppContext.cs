using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Fileware.Models;
using Fileware.Views;

namespace Fileware;

public static class AppContext
{
    public static FileChat ChatInstance;
    public static MainWindow WindowInstance;
    public static Dictionary<int, StoredFileMeta> LocalStoredFiles;
    public const string StorageDir = "storage/";
    public static User CurrentUser { get; set; }

    public static void Save()
    {
        File.WriteAllText("storedFiles.json", JsonSerializer.Serialize(LocalStoredFiles));
    }

    static AppContext()
    {
        try
        {
            if (File.Exists("storedFiles.json"))
                LocalStoredFiles =
                    JsonSerializer.Deserialize<Dictionary<int, StoredFileMeta>>(File.ReadAllText("storedFiles.json"));
            else
            {
                using var file = File.Create("storedFiles.json");
                file.Write("[]"u8.ToArray(), 0, 2);
                LocalStoredFiles = new Dictionary<int, StoredFileMeta>();
            }
        }
        catch (Exception e)
        {
            LocalStoredFiles = new Dictionary<int, StoredFileMeta>();
        }
    }
}

public class StoredFileMeta
{
    public string Path { get; set; }
    public DateTime LastChangeTime { get; set; }
    public int Version { get; set; }
}