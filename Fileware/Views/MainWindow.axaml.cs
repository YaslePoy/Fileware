using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Fileware.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragEnterEvent, DragOver);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, DragDropEvent);
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
        // Api.Http.PostAsync(Api.ApiUrl, new StreamContent());
        if (e.Data.GetFiles() is { } fileNames)
        {
            foreach (var file in fileNames)
            {
                var name = file.Name;
                using var multipartFormContent = new MultipartFormDataContent();
                var fileStreamContent = new StreamContent(File.OpenRead(file.Path.AbsolutePath));
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypes.GetMimeType(name));
                multipartFormContent.Add(fileStreamContent, "file", name);
                using var response = await Api.Http.PostAsync(Api.ApiUrl + "File", multipartFormContent);
            }
        }
    }

    private void SendClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MsgBox.Text))
            return;

        Api.Http.PostAsync(Api.ApiUrl + "Messaging",
            new StringContent("\"" + MsgBox.Text + "\"", MediaTypeWithQualityHeaderValue.Parse("application/json")));
        MsgBox.Text = String.Empty;
    }
}