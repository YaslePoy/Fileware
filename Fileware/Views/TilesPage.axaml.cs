using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Fileware.Controls;
using Fileware.Models;
using Fileware.ViewModels;
using ReactiveUI;

namespace Fileware.Views;

public partial class TilesPage : ReactiveUserControl<TilesViewModel>, IMultiLevelView
{
    private static TilesPage Instance;
    private static List<HistoryPoint> History;


    private static FileData vm;
    private static Tag ColoringTag;

    private readonly FrozenDictionary<string, Action<object>> TopLevelActions =
        new Dictionary<string, Action<object>>
        {
            {
                "FileRename", s =>
                {
                    Instance.RenamingPanel.IsVisible = true;
                    vm = s as FileData;
                    var winVm = new RenameViewModel { FileName = vm.Name };
                    Instance.RenamingPanel.DataContext = winVm;
                }
            },
            {
                "RecolorTag", s =>
                {
                    Instance.RecolorPanel.IsVisible = true;
                    ColoringTag = s as Tag;
                }
            },
            {
                "TagManager", s =>
                {
                    Instance.TagManagerPanel.IsVisible = true;
                    Instance.TagManagerPanel.DataContext = new TagEditorViewModel
                        { CurrentTagsOwner = s as ITagContainer, AllTags = ["Избранное", "Секретное"] };
                }
            }
        }.ToFrozenDictionary();

    private IBrush _defaultBrush;
    private KeyEventArgs? _firstEnter;
    private Avalonia.Controls.Controls _historyBackup;

    public TilesPage()
    {
        Instance = this;
        this.WhenActivated(disposables => { });
        AppContext.CurrentMultiLevelView = this;
        InitializeComponent();
        AppContext.CurrentUser.UserData.PropertyChanged += OnUserDataOnPropertyChanged;

        UpdateFileSpaceSelector();

        FileSpacesComboBox.SelectedIndex = 0;

        // if (File.Exists("./Cache/testHistory.json"))
        // {
        //     var history = File.ReadAllText("./Cache/testHistory.json");
        //     History = JsonSerializer.Deserialize<List<HistoryPoint>>(history, Api.JsonOptions) ??
        //               new List<HistoryPoint>();
        //     ShowHistory();
        // }
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
        var lastPoint = default(HistoryPoint);
        var dontAdd = true;
        PointsPanel.Children.Clear();
        foreach (var point in History)
        {
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
                        {
                            DataContext = fileData, MaxHeight = 350, MaxWidth = 350, Host = this,
                            Margin = new Thickness(10)
                        };
                    }
                    else
                    {
                        adding = new FileBlock
                            { DataContext = fileData, Width = 350, Host = this, Margin = new Thickness(10) };
                    }

                    if (AppContext.LocalStoredFiles.ContainsKey(fileData.Id))
                        (adding as IFileBlock).StartVersionCheckerTimer();

                    break;

                case (int)HistoryPointType.Message:
                    adding = new MessageBlock
                        { DataContext = point.Linked.Deserialize<Message>(Api.JsonOptions) };
                    break;
            }

            if (adding == null) continue;
            PointsPanel.Children.Insert(0, adding);
            (adding.DataContext as ITagContainer)!.Tags = point.Tags.Select(ViewModels.Tag.FromName).ToList();
            (adding.DataContext as ITagContainer)!.PointId = point.Id;
        }
    }

    private void OnCancelRename(object? sender, RoutedEventArgs e)
    {
        RenamingPanel.IsVisible = false;
    }

    private void OnApplyRename(object? sender, RoutedEventArgs e)
    {
        vm.Name = (Instance.RenamingPanel.DataContext as RenameViewModel).FileName;
        vm.OnPropertyChanged("Name");
        RenamingPanel.IsVisible = false;

        Api.Http.PatchAsync($"api/File/{vm.Id}/rename",
            new StringContent("\"" + vm.Name + "\"",
                MediaTypeWithQualityHeaderValue.Parse("application/json")));
    }

    private void OnCancelRecolor(object? sender, RoutedEventArgs e)
    {
        RecolorPanel.IsVisible = false;
    }

    private void OnApplyRecolor(object? sender, RoutedEventArgs e)
    {
        RecolorPanel.IsVisible = false;
        ColoringTag.Color = new SolidColorBrush(RecolorColorPicker.Color);
    }


    private void OnCancelTagAdd(object? sender, RoutedEventArgs e)
    {
        TagManagerPanel.IsVisible = false;
    }

    private void OnApplyTagAdd(object? sender, RoutedEventArgs e)
    {
        var currentContext = TagManagerPanel.DataContext as TagEditorViewModel;
        var nextTags = new List<Tag>(currentContext.CurrentTagsOwner.Tags);
        nextTags.Add(ViewModels.Tag.FromName(currentContext.CurrentTagName));
        currentContext.CurrentTagsOwner.Tags = nextTags;
        TagManagerPanel.IsVisible = false;
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
            File.WriteAllText("./Cache/testHistory.json", t.Result);
            History = JsonSerializer.Deserialize<List<HistoryPoint>>(history, Api.JsonOptions) ??
                      new List<HistoryPoint>();

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
}

public abstract class IFileBlock : UserControl
{
    public void StartVersionCheckerTimer()
    {
    }
}