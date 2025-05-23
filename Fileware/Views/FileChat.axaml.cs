using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Fileware.Controls;
using Fileware.Models;
using Fileware.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;

namespace Fileware.Views;

public partial class FileChatPage : ReactiveUserControl<FileChatPageViewModel>, IMultiLevelView
{
    private static FileChatPage Instance;
    private static List<HistoryPoint> History;

    public HubConnection updateHub;
    private static FileData vm;
    private static Tag ColoringTag;

    private readonly FrozenDictionary<string, Action<object>> TopLevelActions =
        new Dictionary<string, Action<object>>
        {
            {
                "FileRename", s =>
                {
                    Instance.Viewer.Effect = new ImmutableBlurEffect(15);
                    Instance.RenamingPanel.IsVisible = true;
                    vm = s as FileData;
                    var winVm = new RenameViewModel { AnyName = vm.Name };
                    Instance.RenamingPanel.DataContext = winVm;
                }
            },
            {
                "RecolorTag", s =>
                {
                    Instance.Viewer.Effect = new ImmutableBlurEffect(15);
                    Instance.RecolorPanel.IsVisible = true;
                    ColoringTag = s as Tag;
                }
            },
            {
                "TagManager", s =>
                {
                    Instance.Viewer.Effect = new ImmutableBlurEffect(15);
                    Instance.TagManagerPanel.IsVisible = true;
                    var tags = Instance.PointsPanel.Children.Where(i => i.DataContext is ITagContainer)
                        .Select(i => i.DataContext as ITagContainer).SelectMany(i => i.Tags.Select(j => j.Name))
                        .ToList().Also(l => l.AddRange(["Избранное", "Секретное"])).Distinct().ToList();
                    Instance.TagManagerPanel.DataContext = new TagEditorViewModel
                        { CurrentTagsOwner = s as ITagContainer, AllTags = tags };
                }
            },
            {
                "MessageEdit", s =>
                {
                    Instance.Viewer.Effect = new ImmutableBlurEffect(15);
                    Instance.MessageEditPanel.IsVisible = true;
                    messageVm = s as Message;
                    var wimVm = new RenameViewModel { AnyName = messageVm.Text };
                    Instance.MessageEditPanel.DataContext = wimVm;
                }
            }
        }.ToFrozenDictionary();

    private IBrush _defaultBrush;
    private KeyEventArgs? _firstEnter;
    private Avalonia.Controls.Controls _historyBackup;
    private static Message messageVm;

    public FileChatPage()
    {
        AppContext.CurrentMultiLevelView = this;
        Instance = this;
        InitializeComponent();
        this.WhenActivated(_ => { });

        AppContext.CurrentUser.UserData.PropertyChanged += OnUserDataOnPropertyChanged;

        UpdateFileSpaceSelector();

        FileSpacesComboBox.SelectedIndex = 0;

        AppContext.ChatPageInstance = this;
        AddHandler(DragDrop.DragEnterEvent, DragOver);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, DragDropEvent);
    }

    private string _currentFileSpace => (FileSpacesComboBox.SelectedItem as FileSpace).Id;

    public void MakeTopLevel(string key, object sender)
    {
        TopLevelActions[key](sender);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        AppContext.CurrentUser.UserData.PropertyChanged -= OnUserDataOnPropertyChanged;
    }

    private void OnUserDataOnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != "AttachedFileSpaces") return;
        UpdateFileSpaceSelector();
    }


    private void UpdateFileSpaceSelector()
    {
        FileSpacesComboBox.Items.Clear();
        foreach (var userDataAttachedFileSpace in AppContext.CurrentUser.UserData.AttachedFileSpaces)
            FileSpacesComboBox.Items.Add(new FileSpace { Id = userDataAttachedFileSpace });

        FileSpacesComboBox.SelectedIndex = 0;
    }

    private void ShowHistory()
    {
        var lastDate = DateTime.Today;
        var lastPoint = default(HistoryPoint);
        var dontAdd = true;
        PointsPanel.Children.Clear();
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
                            var stream = await t.Result.Content.ReadAsStreamAsync();
                            var ms = new MemoryStream();
                            await stream.CopyToAsync(ms);
                            fileData.PreviewData = ms.ToArray();
                            Dispatcher.UIThread.Invoke(() => { fileData.OnPropertyChanged(nameof(fileData.Preview)); });
                        });
                        adding = new ImageBlock
                            { MaxWidth = 400, DataContext = fileData, MaxHeight = 300.0, Host = this };
                    }
                    else
                    {
                        adding = new FileBlock
                            { DataContext = fileData, Width = 350, Host = this };
                    }

                    if (AppContext.LocalStoredFiles.ContainsKey(fileData.Id))
                        (adding as IFileBlock).StartVersionCheckerTimer();

                    break;

                case (int)HistoryPointType.Message:
                    adding = new MessageBlock
                        { DataContext = point.Linked.Deserialize<Message>(Api.JsonOptions) };
                    break;
            }

            if (adding != null)
            {
                PointsPanel.Children.Insert(0, adding);
                (adding.DataContext as ITagContainer).Tags = point.Tags.Select(ViewModels.Tag.FromName).ToList();
                (adding.DataContext as ITagContainer).PointId = point.Id;
            }

            lastPoint = point;
            dontAdd = false;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            if (lastPoint != null)
            {
                var dateLine = new DateLine { DataContext = lastPoint.Time };
                PointsPanel.Children.Insert(0, dateLine);
            }

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
            foreach (var file in fileNames)
                await SendFile(new FileInfo(file.Path.LocalPath));
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

        IFileBlock addedBlock = null;

        Dispatcher.UIThread.Invoke(() =>
        {
            var data = new FileData
            {
                FileType = mimeType, Version = 1, LoadTime = DateTime.Now,
                Size = info.Length, LastChange = DateTime.Now, Id = 0,
                Name = name
            };

            if (data.FileType.StartsWith("image")) data.PreviewData = File.ReadAllBytes(info.FullName);

            addedBlock = data.FileType.StartsWith("image/png") || data.FileType.StartsWith("image/jpeg") ||
                         data.FileType.StartsWith("image/webp")
                ? new ImageBlock { DataContext = data, Host = this, MaxHeight = 300.0, MaxWidth = 400 }
                : new FileBlock(fileStream, data) { Host = this };
            addedBlock.StartVersionCheckerTimer();
            PointsPanel.Children.Add(addedBlock);
            Viewer.ScrollToEnd();
        });
        try
        {
            using var response = await Api.Http.PostAsync(
                info.Length > 30 * 1024 * 1024
                    ? $"api/File/large?fileSpace={HttpUtility.UrlEncode(_currentFileSpace)}"
                    : $"api/File?fileSpace={HttpUtility.UrlEncode(_currentFileSpace)}",
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

        Api.Http.PostAsync($"api/Messaging?fileSpace={HttpUtility.UrlEncode(_currentFileSpace)}",
                new StringContent("\"" + MsgBox.Text + "\"", MediaTypeWithQualityHeaderValue.Parse("application/json")))
            .ContinueWith(async t =>
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
        MsgBox.Text = string.Empty;
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

    private void MsgBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_firstEnter is not null && e.PhysicalKey == _firstEnter.PhysicalKey &&
            e.KeyModifiers == _firstEnter.KeyModifiers)
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
        if (!Directory.Exists("./Cache")) Directory.CreateDirectory("./Cache");

        var now = DateTime.Now;
        var name = $"clipboard_{now:MM_dd_yyyy_hh_mm_ss}.png";
        var path = "./Cache/" + name;
        File.WriteAllBytes(path, pngData);
        // return path;
        return new FileInfo(path);
    }

    private void AttachFile(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            { Title = "Выберите файл", AllowMultiple = false }).ContinueWith(async t =>
        {
            foreach (var file in t.Result) await SendFile(new FileInfo(file.Path.LocalPath));
        });
    }

    private void OnCancelRename(object? sender, RoutedEventArgs e)
    {
        RenamingPanel.IsVisible = false;
        Viewer.Effect = null;
    }

    private void OnApplyRename(object? sender, RoutedEventArgs e)
    {
        vm.Name = (Instance.RenamingPanel.DataContext as RenameViewModel).AnyName;
        vm.OnPropertyChanged("Name");
        RenamingPanel.IsVisible = false;
        Viewer.Effect = null;

        Api.Http.PatchAsync($"api/File/{vm.Id}/rename",
            new StringContent("\"" + vm.Name + "\"",
                MediaTypeWithQualityHeaderValue.Parse("application/json")));
    }

    private void OnCancelRecolor(object? sender, RoutedEventArgs e)
    {
        RecolorPanel.IsVisible = false;
        Viewer.Effect = null;
    }

    private void OnApplyRecolor(object? sender, RoutedEventArgs e)
    {
        RecolorPanel.IsVisible = false;
        Viewer.Effect = null;
        ColoringTag.Color = new SolidColorBrush(RecolorColorPicker.Color);
    }


    private void OnCancelTagAdd(object? sender, RoutedEventArgs e)
    {
        TagManagerPanel.IsVisible = false;
        Viewer.Effect = null;
    }

    private void OnApplyTagAdd(object? sender, RoutedEventArgs e)
    {
        var currentContext = TagManagerPanel.DataContext as TagEditorViewModel;
        var nextTags = new List<Tag>(currentContext.CurrentTagsOwner.Tags);
        nextTags.Add(ViewModels.Tag.FromName(currentContext.CurrentTagName));
        currentContext.CurrentTagsOwner.Tags = nextTags;
        TagManagerPanel.IsVisible = false;
        Viewer.Effect = null;
    }

    private void UpdateTagAddFieldColor(object? sender, TextChangedEventArgs e)
    {
        var textBox = sender as AutoCompleteBox;
        if (!string.IsNullOrWhiteSpace(textBox.Text))
        {
            var currentContext = TagManagerPanel.DataContext as TagEditorViewModel;
            ApplyTagAddButton.IsEnabled = !currentContext.CurrentTagsOwner.Tags.Any(i => i.Name == textBox.Text);
            TagColorIndicator.Background = new SolidColorBrush(TagColorService.GetColorByString(textBox.Text));
        }
        else
        {
            TagColorIndicator.Background = (IBrush?)Application.Current.Resources["MainColorBrush"];
            ApplyTagAddButton.IsEnabled = false;
        }

        Viewer.Effect = null;
    }

    private void AddFileSpace(object? sender, RoutedEventArgs e)
    {
        var vm = DataContext as FileChatPageViewModel;
        vm.HostScreen.Router.Navigate.Execute(new FileSpaceViewModel(vm.HostScreen));
    }

    private void UpdateFileSpace(object? sender, SelectionChangedEventArgs e)
    {
        if (FileSpacesComboBox.SelectedValue is null)
            return;
        Api.Http.GetStringAsync($"api/History?id=-1&count=100&key={_currentFileSpace}").ContinueWith(t =>
        {
            var history = t.Result;
            History = JsonSerializer.Deserialize<List<HistoryPoint>>(history, Api.JsonOptions) ??
                      new List<HistoryPoint>();

            updateHub = new HubConnectionBuilder().WithUrl($"{Api.ApiUrl}/ChangesHub").Build();
            updateHub.InvokeAsync("AttachToFilespace", _currentFileSpace);
            updateHub.On<int>("NotifyFileUpdate", async fileId =>
            {
                var file = PointsPanel.Children.Select(i => i.DataContext).Where(i => i is FileData).Cast<FileData>()
                    .FirstOrDefault(i => i.Id == fileId);
                if (file is null)
                {
                    return;
                }

                var upadte = await Api.Http.GetStringAsync($"api/File/{fileId}");
                var ser = JsonSerializer.Deserialize<FileData>(upadte, Api.JsonOptions);
                Utils.TransferData(file, ser, false);
            });

            updateHub.On<int>("NotifyMessageUpdate", async fileId =>
            {
                var file = PointsPanel.Children.Select(i => i.DataContext).Where(i => i is Message).Cast<FileData>()
                    .FirstOrDefault(i => i.Id == fileId);
                if (file is null)
                {
                    return;
                }

                var upadte = await Api.Http.GetStringAsync($"api/Message/{fileId}");
                var ser = JsonSerializer.Deserialize<Message>(upadte, Api.JsonOptions);
                Utils.TransferData(file, ser);
            });
            Dispatcher.UIThread.Invoke(ShowHistory);
        });
    }

    private void SearchText(object? sender, TextChangedEventArgs e)
    {
        var text = (sender as TextBox)!.Text;

        if (_historyBackup is null) _historyBackup = new Avalonia.Controls.Controls(PointsPanel.Children);

        if (string.IsNullOrWhiteSpace(text))
        {
            PointsPanel.Children.Clear();
            PointsPanel.Children.AddRange(_historyBackup);
            _historyBackup = null;
            return;
        }

        IEnumerable<Control> finalQuery;

        if (text.StartsWith("tag="))
        {
            var tag = text.Substring("tag=".Length);
            if (tag.Length == 0)
                finalQuery = [];
            else
                finalQuery = _historyBackup.Where(i =>
                    i.DataContext is ITagContainer tagContainer && tagContainer.Tags.Any(t =>
                        t.Name.Contains(tag, StringComparison.InvariantCultureIgnoreCase)));
        }
        else
        {
            finalQuery = _historyBackup.Where(i => i.DataContext is ISearchable searchable && searchable.IsSuits(text));
        }

        PointsPanel.Children.Clear();
        PointsPanel.Children.AddRange(finalQuery);
    }

    private void OnCancelMessage(object? sender, RoutedEventArgs e)
    {
        MessageEditPanel.IsVisible = false;
        Viewer.Effect = null;
    }

    private void OnApplyMessage(object? sender, RoutedEventArgs e)
    {
        MessageEditPanel.IsVisible = false;
        Viewer.Effect = null;
        messageVm.Text = (Instance.MessageEditPanel.DataContext as RenameViewModel).AnyName;
        messageVm.OnPropertyChanged("Text");

        Api.Http.PatchAsync($"api/Messaging/{messageVm.Id}",
            new StringContent("\"" + messageVm.Text + "\"",
                MediaTypeWithQualityHeaderValue.Parse("application/json")));
    }
}