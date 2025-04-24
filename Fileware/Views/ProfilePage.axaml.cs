using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Fileware.ViewModels;
using ReactiveUI;

namespace Fileware.Views;

public partial class ProfilePage : ReactiveUserControl<ProfileViewModel>
{
    public ProfilePage()
    {
        this.WhenActivated(_ => { });
        InitializeComponent();
    }

    private void EditProfile(object? sender, TappedEventArgs e)
    {
        var vm = DataContext as ProfileViewModel;
        vm.HostScreen.Router.Navigate.Execute(new EditProfileViewModel(vm.HostScreen, vm.CommonUser));
    }

    private void Exit(object? sender, TappedEventArgs e)
    {
        Directory.Delete("./UserData", true);
        AppContext.CurrentUser = null;
        (AppContext.WindowInstance.DataContext as MainWindowViewModel).Router.Navigate
            .Execute(new StartPageViewModel(AppContext.WindowInstance.DataContext as MainWindowViewModel));
    }

    private void EditName(object? sender, RoutedEventArgs e)
    {
        var dc = DataContext as ProfileViewModel;
        RenamingPanel.DataContext = new RenameViewModel { AnyName = dc.CommonUser.ShowName };
        RenamingPanel.IsVisible = true;
        MainPanel.Effect = new ImmutableBlurEffect(15);
    }

    private void EditDate(object? sender, RoutedEventArgs e)
    {
        var dc = DataContext as ProfileViewModel;
        RedatePanel.DataContext = new RenameViewModel { AnyName = dc.CommonUser.BirthDate.ToString("MM.dd.yyyy") };
        RedatePanel.IsVisible = true;
        MainPanel.Effect = new ImmutableBlurEffect(15);
    }

    private void EditAvatar(object? sender, RoutedEventArgs e)
    {
        TopLevel.GetTopLevel(this)
            ?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false, FileTypeFilter = [FilePickerFileTypes.ImageAll],
            Title = "Выберите вашу новую аватарку"
        }).ContinueWith(async t =>
        {
            var file = t.Result[0];
            var stream = await file.OpenReadAsync();
            using var multipartFormContent = new MultipartFormDataContent();
            var fileStreamContent = new StreamContent(stream);
            var mimeType = MimeTypes.GetMimeType(file.Name);
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            multipartFormContent.Add(fileStreamContent, "avatar",file.Name);
            var result = await Api.Http.PostAsync($"api/User/{AppContext.CurrentUser.UserData.Id}/avatar", multipartFormContent);
            Debug.Print(result.IsSuccessStatusCode.ToString());
            Dispatcher.UIThread.Invoke((() =>
            {
                AppContext.CurrentUser.UserData.OnPropertyChanged("AvatarImage");
            }));
        });
    }

    private void OnCancelRename(object? sender, RoutedEventArgs e)
    {
        RenamingPanel.IsVisible = false;
        RedatePanel.IsVisible = false;
        MainPanel.Effect = null;

    }

    private void OnApplyRename(object? sender, RoutedEventArgs e)
    {
        if (RenamingPanel.IsVisible)
        {
            var vm = RenamingPanel.DataContext as RenameViewModel;

            var curvm = DataContext as ProfileViewModel;
            curvm.CommonUser.ShowName = vm.AnyName;
            AppContext.CurrentUser.Save();
            RenamingPanel.IsVisible = false;
        }
        else if (RedatePanel.IsVisible)
        {
            var vm = RedatePanel.DataContext as RenameViewModel;

            var curvm = DataContext as ProfileViewModel;
            if (!DateOnly.TryParse(vm.AnyName, out var dateOnly))
            {
                vm.AlertText = "Введите корректную дату";
                return;
            }

            curvm.CommonUser.BirthDate = dateOnly;
            AppContext.CurrentUser.Save();
            RedatePanel.IsVisible = false;
        }

        MainPanel.Effect = null;
    }
}