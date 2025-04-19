﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Fileware.Models;
using Fileware.Views;

namespace Fileware;

public static class AppContext
{
    public const string StorageDir = "storage/";
    public static FileChatPage ChatPageInstance;
    public static MainWindow WindowInstance;
    public static IMultiLevelView CurrentMultiLevelView;
    public static Dictionary<int, StoredFileMeta> LocalStoredFiles;
    public static LoginResponse? CurrentUser;
    static AppContext()
    {
        try
        {
            if (File.Exists("storedFiles.json"))
            {
                LocalStoredFiles =
                    JsonSerializer.Deserialize<Dictionary<int, StoredFileMeta>>(File.ReadAllText("storedFiles.json"));
            }
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
    
    public static void Save()
    {
        File.WriteAllText("storedFiles.json", JsonSerializer.Serialize(LocalStoredFiles));
    }
}

public class StoredFileMeta
{
    public string Path { get; set; }
    public DateTime LastChangeTime { get; set; }
    public int Version { get; set; }
}