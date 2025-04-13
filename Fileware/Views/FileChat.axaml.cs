using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Fileware.Controls;
using Fileware.Models;
using Fileware.ViewModels;
using ReactiveUI;

namespace Fileware.Views;

public partial class FileChat : ReactiveUserControl<FileChatViewModel>
{
    private static List<HistoryPoint> History;

    public FileChat()
    {
        InitializeComponent();
        this.WhenActivated(disposables => { });

        AppContext.ChatInstance = this;
        AddHandler(DragDrop.DragEnterEvent, DragOver);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, DragDropEvent);

        Api.Http.GetStringAsync("api/History?id=-1&count=100").ContinueWith(t =>
        {
            var history = t.Result;
            History = JsonSerializer.Deserialize<List<HistoryPoint>>(history, Api.JsonOptions) ??
                      new List<HistoryPoint>();
            Dispatcher.UIThread.Invoke(ShowHistory);
        });
    }

    private void ShowHistory()
    {
        var lastDate = DateTime.Today;
        var lastPoint = default(HistoryPoint);
        bool dontAdd = true;
        foreach (var point in History)
        {
            if (point.Time.Date != DateTime.Today && point.Time.Date != lastDate)
            {
                if (!dontAdd)
                {
                    var dateLine = new DateLine
                        { DataContext = lastDate, HorizontalAlignment = HorizontalAlignment.Center };
                    PointsPanel.Children.Insert(0, dateLine);
                }

                lastDate = point.Time.Date;
            }

            Control adding = default;

            switch (point.Type)
            {
                case (int)HistoryPointType.File:

                    var fileData = point.Linked.Deserialize<FileData>(Api.JsonOptions);
                    if (fileData.HasPreview)
                    {
                        Api.Http.GetAsync($"api/File/{fileData.Id}/preview").ContinueWith(async t =>
                        {
                            // return;
                            var stream = await t.Result.Content.ReadAsStreamAsync();
                            fileData.Preview = new Bitmap(stream);
                            Dispatcher.UIThread.Invoke(() => { fileData.OnPropertyChanged(nameof(fileData.Preview)); });
                        });
                        adding = new ImageBlock { DataContext = fileData };
                    }
                    else
                        adding = new FileBlock
                            { DataContext = fileData };

                    if (AppContext.LocalStoredFiles.ContainsKey(fileData.Id))
                    {
                        (adding as FileBlock).StartVersionCheckerTimer();
                    }

                    break;

                case (int)HistoryPointType.Message:
                    adding = new MessageBlock
                        { DataContext = point.Linked.Deserialize<Message>(Api.JsonOptions) };
                    break;
            }

            if (adding != null) PointsPanel.Children.Insert(0, adding);

            lastPoint = point;
            dontAdd = false;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            var dateLine = new DateLine { DataContext = lastPoint.Time };
            PointsPanel.Children.Insert(0, dateLine);
            Viewer.ScrollToEnd();
        });
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        Viewer.Effect = new ImmutableBlurEffect(15);
        LoadPanel.IsVisible = true;
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Viewer.Effect = null;
        LoadPanel.IsVisible = false;
    }

    private async void DragDropEvent(object? sender, DragEventArgs e)
    {
        Viewer.Effect = null;
        LoadPanel.IsVisible = false;
        if (e.Data.GetFiles() is { } fileNames)
        {
            foreach (var file in fileNames)
            {
                await SendFile(new FileInfo(file.Path.LocalPath));
            }
        }
    }

    private async Task SendFile(FileInfo info)
    {
        var name = info.Name;
        using var multipartFormContent = new MultipartFormDataContent();
        var fileStream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite);
        var fileStreamContent = new StreamContent(fileStream);
        var mimeType = MimeTypes.GetMimeType(name);
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        multipartFormContent.Add(fileStreamContent, "file", name);

        FileBlock addedBlock = null;

        Dispatcher.UIThread.Invoke(() =>
        {
            var data = new FileData
            {
                FileType = mimeType, Version = 1, LoadTime = DateTime.Now,
                Size = info.Length, LastChange = DateTime.Now, Id = 0,
                Name = name
            };

            if (data.FileType.StartsWith("image"))
            {
                data.PreviewData = File.ReadAllBytes(info.FullName);
            }

            addedBlock = data.FileType.StartsWith("image")
                ? new ImageBlock { DataContext = data }
                : new FileBlock(fileStream, data);
            addedBlock.StartVersionCheckerTimer();
            PointsPanel.Children.Add(addedBlock);
            Viewer.ScrollToEnd();
        });
        try
        {
            using var response = await Api.Http.PostAsync(
                info.Length > 30 * 1024 * 1024 ? "api/File/large" : "api/File",
                multipartFormContent);
            var id = int.Parse(await response.Content.ReadAsStringAsync());
            AppContext.LocalStoredFiles.Add(id,
                new StoredFileMeta
                    { Path = info.FullName, LastChangeTime = info.LastWriteTime, Version = 1 });
            AppContext.Save();
            (addedBlock.DataContext as FileData).Id = id;

            History.Add(new HistoryPoint
                { LinkedId = id, Time = DateTime.Now, Type = (int)HistoryPointType.File });
        }
        catch (Exception)
        {
        }
    }

    private void SendText()
    {
        if (string.IsNullOrWhiteSpace(MsgBox.Text))
            return;

        var savedText = MsgBox.Text;

        Api.Http.PostAsync("api/Messaging",
                new StringContent("\"" + MsgBox.Text + "\"", MediaTypeWithQualityHeaderValue.Parse("application/json")))
            .ContinueWith(
                async (t) =>
                {
                    var id = int.Parse(await t.Result.Content.ReadAsStringAsync());
                    History.Add(new HistoryPoint
                        { LinkedId = id, Time = DateTime.Now, Type = (int)HistoryPointType.Message });

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        PointsPanel.Children.Add(new MessageBlock
                            { DataContext = new Message { Text = savedText, Time = DateTime.Now, Id = id } });
                        Viewer.ScrollToEnd();
                    });
                });
        MsgBox.Text = String.Empty;
        Viewer.ScrollToEnd();
    }

    private void SendClick(object? sender, RoutedEventArgs e)
    {
        SendText();
    }

    private void Viewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (Viewer.Offset.Y != 0)
            return;
    }

    private KeyEventArgs? _firstEnter;

    private void MsgBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_firstEnter is not null && e.PhysicalKey == _firstEnter.PhysicalKey && e.KeyModifiers == _firstEnter.KeyModifiers)
        {
            _firstEnter = null;
            return;
        }

        _firstEnter = e;
        if (e is { PhysicalKey: PhysicalKey.Enter, KeyModifiers: KeyModifiers.Shift })
        {
           MsgBox.Text += "\n";
            MsgBox.CaretIndex = MsgBox.CaretIndex + 1;
        }
        else if (e.PhysicalKey == PhysicalKey.Enter)
        {
            SendText();
        }
        else if (e is { PhysicalKey: PhysicalKey.V, KeyModifiers: KeyModifiers.Control })
        {
            var clipboard = TopLevel.GetTopLevel(sender as Visual).Clipboard;
            clipboard.GetFormatsAsync().ContinueWith(task =>
            {
                var png = task.Result.First(i => i.EndsWith(".png"));
                var obj = clipboard.GetDataAsync(png).ContinueWith(it =>
                {
                    SendFile(SaveImageToCache(it.Result as byte[]));
                });
                Debug.Print(obj.ToString());
            });
        }
    }

    private FileInfo SaveImageToCache(byte[] pngData)
    {
        if (!Directory.Exists("./Cache"))
        {
            Directory.CreateDirectory("./Cache");
        }

        var now = DateTime.Now;
        var name = $"clipboard_{now.ToString("MM_dd_yyyy_hh_mm_ss")}.png";
        var path = "./Cache/" + name;
        File.WriteAllBytes(path, pngData);
        // return path;
        return new FileInfo(path);
    }
}