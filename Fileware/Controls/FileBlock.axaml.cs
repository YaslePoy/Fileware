using System;
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
using Fileware.Views;
using Fileware.Windows;

namespace Fileware.Controls;

public partial class FileBlock : UserControl
{
    public readonly FileStream SendingStream;

    public FileBlock()
    {
        InitializeComponent();
    }

    public FileBlock(FileStream stream, FileData data) : this()
    {
        SendingStream = stream;
        UploadBar.IsVisible = true;
        var timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(100);
        timer.Tick += (sender, args) =>
        {
            if (!stream.CanRead)
            {
                timer.Stop();
                UploadBar.IsVisible = false;
            }

            var progress = stream.Position / (double)data.Size * 100;
            UploadBar.Value = progress;
            if (stream.Position == data.Size)
            {
                timer.Stop();
                UploadBar.IsVisible = false;
            }
        };
        timer.Start();
    }

    private void OnRename(object? sender, RoutedEventArgs e)
    {
        var win = new FileRenameWindow { DataContext = DataContext };
        win.ShowDialog<bool>(AppContext.MainWindow).ContinueWith(t =>
        {
            if (t.Result)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    var msg = DataContext as FileData;
                    msg.OnPropertyChanged("Name");
                    Api.Http.PatchAsync(Api.ApiUrl + $"api/File/{msg.Id}/rename",
                        new StringContent("\"" + msg.Name + "\"",
                            MediaTypeWithQualityHeaderValue.Parse("application/json")));
                });
            }
        });
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        var current = DataContext as FileData;
        Api.Http.DeleteAsync($"{Api.ApiUrl}api/File/{current.Id}");
        AppContext.MainWindow.PointsPanel.Children.Remove(this);
    }

    void OpenFile()
    {
        var file = DataContext as FileData;

        Process.Start(new ProcessStartInfo { FileName = AppContext.LocalStoredFiles[file.Id], UseShellExecute = true });
    }

    void LoadFile(string directory)
    {
        var fileData = DataContext as FileData;
        var streamTask = Api.Http.GetStreamAsync($"{Api.ApiUrl}api/File/{fileData.Id}/load");
        streamTask.Wait();
        var stream = streamTask.Result;
        var location = directory + fileData.Name;
        var fileStream = File.Create(location);
        location = new FileInfo(location).FullName;
        stream.CopyToAsync(fileStream);
        UploadBar.IsVisible = true;
        var timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(100);
        timer.Tick += (sender, args) =>
        {
            void finish()
            {
                stream.Dispose();
                fileStream.Dispose();
                timer.Stop();
                UploadBar.IsVisible = false;
                AppContext.LocalStoredFiles.Add(fileData.Id, location);
                AppContext.SaveFileList();
            }

            if (!fileStream.CanRead)
            {
                finish();
                return;
            }

            var progress = fileStream.Position / (double)fileData.Size * 100;
            UploadBar.Value = progress;
            if (fileStream.Position == fileData.Size)
            {
                finish();
                return;
            }
        };
        timer.Start();
    }

    void LoadToDefaultDir()
    {
        Directory.CreateDirectory(AppContext.StorageDir);
        LoadFile(AppContext.StorageDir);
    }

    async Task LoadToSelectionDir()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var dir = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            { Title = "Выберите расположение для файла" });
        LoadFile(dir.First().Path.LocalPath);
    }

    private void OnDoubleTap(object? sender, TappedEventArgs e)
    {
        var file = DataContext as FileData;
        if (AppContext.LocalStoredFiles.TryGetValue(file.Id, out string path))
        {
            OpenFile();
        }
        else
        {
            LoadToDefaultDir();
        }
    }

    private void InputElement_OnTapped(object? sender, TappedEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control)
            LoadToSelectionDir();
        else if (e.KeyModifiers == KeyModifiers.Shift)
            LoadToDefaultDir();
    }
}