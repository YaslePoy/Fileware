﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Fileware.Models;
using Fileware.ViewModels;
using Fileware.Windows;

namespace Fileware.Controls;

public partial class FileBlock : UserControl
{
    public bool IsCheckUpdates;
    public IMultiLevelView Host;
    private bool IsTransfering;

    public FileBlock()
    {
        InitializeComponent();
    }

    public FileBlock(FileStream stream, FileData data) : this()
    {
        DataContext = data;
        ActivateTransferTimer(stream, data.Size, () => { });
    }

    protected void OnRename(object? sender, RoutedEventArgs e)
    {
        Host.MakeTopLevel("FileRename", DataContext);
        
    }

    protected override void OnDataContextEndUpdate()
    {
        base.OnDataContextEndUpdate();
        var data = DataContext as FileData;
        data.TagsPreviewPanel = TagsPanel;
        data.UpdateTagPanel();
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        var current = DataContext as FileData;
        Api.Http.DeleteAsync($"api/File/{current.Id}");
        AppContext.ChatInstance.PointsPanel.Children.Remove(this);
    }

    private void OpenFile()
    {
        var file = DataContext as FileData;

        Process.Start(new ProcessStartInfo
            { FileName = AppContext.LocalStoredFiles[file.Id].Path, UseShellExecute = true });
    }

    private void LoadFile(string directory)
    {
        var fileData = DataContext as FileData;
        var streamTask = Api.Http.GetStreamAsync($"api/File/{fileData.Id}/load");
        streamTask.Wait();
        var stream = streamTask.Result;
        var location = directory + fileData.Name;
        var fileStream = File.Create(location);
        location = new FileInfo(location).FullName;
        stream.CopyToAsync(fileStream);

        ActivateTransferTimer(fileStream, fileData.Size, () =>
        {
            AppContext.LocalStoredFiles.Add(fileData.Id,
                new StoredFileMeta
                {
                    Path = location, LastChangeTime = new FileInfo(location).LastWriteTime, Version = fileData.Version
                });
            AppContext.Save();
            StartVersionCheckerTimer();
        });
    }

    private void ActivateTransferTimer(Stream stream, long total, Action onFinish)
    {
        IsTransfering = true;
        var timer = new DispatcherTimer();

        var fileData = DataContext as FileData;
        fileData.LoadProgressEnable = true;
        fileData.OnPropertyChanged("LoadProgressEnable");

        timer.Interval = TimeSpan.FromMilliseconds(100);

        long lastPosition = 0;
        var iter = 0;
        timer.Tick += (_, _) =>
        {
            iter++;

            void Finish()
            {
                stream.Dispose();
                timer.Stop();
                IsTransfering = false;
                fileData.UpdateSyncState();
                fileData.LoadProgressEnable = false;
                fileData.OnPropertyChanged("LoadProgressEnable");
                onFinish();
            }

            if (!stream.CanRead)
            {
                Finish();
                return;
            }

            var progress = stream.Position / (double)total * 100;
            if (iter == 10)
            {
                var delta = stream.Position - lastPosition;

                var speed = FileData.SpeedFormatted(delta);

                lastPosition = stream.Position;
                SpeedTB.Text = speed;
                iter = 0;
            }

            if (progress == 100)
                Finish();

            UploadBar.Value = progress;
        };
        timer.Start();
    }

    private void LoadToDefaultDir()
    {
        Directory.CreateDirectory(AppContext.StorageDir);
        LoadFile(AppContext.StorageDir);
    }

    private async Task LoadToSelectionDir()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var dir = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            { Title = "Выберите расположение для файла" });
        LoadFile(dir.First().Path.LocalPath);
    }

    public void StartVersionCheckerTimer()
    {
        if (IsCheckUpdates)
            return;
        IsCheckUpdates = true;
        var timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(5);
        timer.Tick += (_, _) =>
        {
            if (!IsCheckUpdates)
            {
                timer.Stop();
                return;
            }

            var fileData = DataContext as FileData;
            fileData.UpdateSyncState();
        };
        timer.Start();
    }

    private void UploadFile()
    {
        var fileData = DataContext as FileData;
        var localStoreData = AppContext.LocalStoredFiles[fileData.Id];
        var path = localStoreData.Path;
        var info = new FileInfo(path);
        fileData.Size = info.Length;
        fileData.Version++;
        fileData.OnPropertyChanged("SizeFormatted");
        fileData.OnPropertyChanged("Version");

        var multipartFormContent = new MultipartFormDataContent();
        var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite);
        var fileStreamContent = new StreamContent(fileStream);
        var mimeType = MimeTypes.GetMimeType(fileData.Name);
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        multipartFormContent.Add(fileStreamContent, "file", fileData.Name);

        Api.Http.PatchAsync($"api/File/large/{fileData.Id}",
            // Api.ApiUrl + (info.Length > 30 * 1024 * 1024 ? $"api/File/large/{fileData.Id}" : $"api/File/{fileData.Id}"),
            multipartFormContent).ContinueWith(async t =>
        {
            var msg = await t.Result.Content.ReadAsStringAsync();
            Console.WriteLine(t.Result.StatusCode);
            multipartFormContent.Dispose();
        });

        ActivateTransferTimer(fileStream, fileData.Size,
            () =>
            {
                AppContext.LocalStoredFiles[fileData.Id] = new StoredFileMeta
                    { Path = localStoreData.Path, LastChangeTime = info.LastWriteTime, Version = fileData.Version };
                AppContext.Save();
                (DataContext as FileData).UpdateSyncState();
            });
    }

    private void OnDoubleTap(object? sender, TappedEventArgs e)
    {
        var file = DataContext as FileData;
        if (AppContext.LocalStoredFiles.ContainsKey(file.Id))
            OpenFile();
        else if (!IsTransfering) LoadToDefaultDir();
    }

    private void InputElement_OnTapped(object? sender, TappedEventArgs e)
    {
        if (IsTransfering)
            return;
        if (e.KeyModifiers == KeyModifiers.Control)
            LoadToSelectionDir();
        else if (e.KeyModifiers == KeyModifiers.Shift)
            LoadToDefaultDir();
    }

    private void UploadNewer(object? sender, TappedEventArgs e)
    {
        if (IsTransfering)
            return;

        UploadFile();
    }

    private void DownloadNewer(object? sender, TappedEventArgs e)
    {
        if (IsTransfering)
            return;

        LoadFile(AppContext.LocalStoredFiles[(DataContext as FileData).Id].Path);
    }

    public void AddTag(object? sender, RoutedEventArgs e)
    {
        AppContext.CurrentMultiLevelView.MakeTopLevel("TagManager", DataContext);
    }
}