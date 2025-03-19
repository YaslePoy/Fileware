using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Fileware.Controls;
using Fileware.Models;

namespace Fileware.Views;

public partial class MainWindow : Window
{
    public static List<HistoryPoint> History;

    public MainWindow()
    {
        InitializeComponent();
        AppContext.MainWindow = this;
        AddHandler(DragDrop.DragEnterEvent, DragOver);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, DragDropEvent);

        Api.Http.GetStringAsync(Api.ApiUrl + "api/History?id=-1&count=100").ContinueWith(t =>
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
                    var dateLine = new DateLine { DataContext = lastDate };
                    PointsPanel.Children.Insert(0, dateLine);
                }

                lastDate = point.Time.Date;
            }

            Control adding = default;

            switch (point.Type)
            {
                case (int)HistoryPointType.File:

                    var fileData = point.Linked.Deserialize<FileData>(Api.JsonOptions);
                    if (!fileData.HasPreview)
                    {
                        Api.Http.GetAsync($"api/File/{fileData.Id}/preview").ContinueWith(async t =>
                        {
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
                var info = new FileInfo(file.Path.LocalPath);
                var name = file.Name;
                using var multipartFormContent = new MultipartFormDataContent();
                var fileStream = new FileStream(file.Path.LocalPath, FileMode.Open, FileAccess.Read,
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
                    addedBlock = new FileBlock(fileStream, data);
                    addedBlock.StartVersionCheckerTimer();
                    PointsPanel.Children.Add(addedBlock);
                    Viewer.ScrollToEnd();
                });

                using var response = await Api.Http.PostAsync(
                    Api.ApiUrl + (info.Length > 30 * 1024 * 1024 ? "api/File/large" : "api/File"),
                    multipartFormContent);
                var id = int.Parse(await response.Content.ReadAsStringAsync());
                AppContext.LocalStoredFiles.Add(id,
                    new StoredFileMeta
                        { Path = file.Path.LocalPath, LastChangeTime = info.LastWriteTime, Version = 1 });
                AppContext.Save();
                (addedBlock.DataContext as FileData).Id = id;

                History.Add(new HistoryPoint
                    { LinkedId = id, Time = DateTime.Now, Type = (int)HistoryPointType.File });
            }
        }
    }

    private void SendClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MsgBox.Text))
            return;

        var savedText = MsgBox.Text;
        Api.Http.PostAsync(Api.ApiUrl + "api/Messaging",
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
    }

    private void Viewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (Viewer.Offset.Y != 0)
            return;
    }
}