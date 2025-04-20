using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;
using Fileware.Models;
using ReactiveUI;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Fileware.ViewModels;

public class FileSpaceViewModel : ReactiveObject, IRoutableViewModel
{
    private bool _confirmVisibility;
    private string _confirmActionText;
    private string _alertText;

    public FileSpaceViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
        GoBackVM = new GoBackViewModel { Base = HostScreen };
    }

    public GoBackViewModel GoBackVM { get; }

    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; set; }
    public bool IsPrivate { get; set; }
    public string FileSpaceName { get; set; }

    public string ConfirmActionText
    {
        get => _confirmActionText;
        set => this.RaiseAndSetIfChanged(ref _confirmActionText, value);
    }

    public bool ConfirmVisibility
    {
        get => _confirmVisibility;
        set => this.RaiseAndSetIfChanged(ref _confirmVisibility, value);
    }


    public ICommand OnTryCreateFileSpace => ReactiveCommand.Create(() =>
    {
        if (string.IsNullOrWhiteSpace(FileSpaceName))
        {
            AlertText = "Введите название файлового пространства";
            return;
        }


        if (AppContext.CurrentUser.UserData.AttachedFileSpaces.Contains(FileSpaceName))
        {
            AlertText = "У вас уже есть такое пространство";
            return;
        }

        ConfirmActionText =
            $"Создание  файлового {(IsPrivate ? "личного" : "публичного")} файлового пространства '{FileSpaceName}'";
        ConfirmVisibility = true;
    });


    public ICommand OnTryAttachFileSpace => ReactiveCommand.Create(() =>
    {
        if (string.IsNullOrWhiteSpace(FileSpaceName))
        {
            AlertText = "Введите идентификатор файлового пространства";
            return;
        }

        var name = Encoding.Default.GetString(Convert.FromBase64String(FileSpaceName));
        if (AppContext.CurrentUser.UserData.AttachedFileSpaces.Contains(name))
        {
            AlertText = "Вы уже состоите в таком файловом пространстве";
            return;
        }

        ConfirmActionText = $"Вступление в файловое пространстно '{new FileSpace() { Id = name }.Display}'";
        ConfirmVisibility = true;
    });

    private static string RandomChatId => RandomNumberGenerator.GetString("qwertyuiopasdfghjklzxcvbnm", 6);

    public string AlertText
    {
        get => _alertText;
        set => this.RaiseAndSetIfChanged(ref _alertText, value);
    }

    public FileSpacePageMode CurrentMode { get; set; }

    public ICommand ConfirmFileSpaceAction => ReactiveCommand.Create(() =>
    {
        switch (CurrentMode)
        {
            case FileSpacePageMode.Attach:
                var attachSpaceKey = Encoding.Default.GetString(Convert.FromBase64String(FileSpaceName));
                AppContext.CurrentUser.UserData.AttachedFileSpaces =
                    new List<string>(AppContext.CurrentUser.UserData.AttachedFileSpaces).Also(l => l.Add(attachSpaceKey));
                HostScreen.Router.NavigateBack.Execute();
                break;
            case FileSpacePageMode.Create:
                var fileSpaceKey = (IsPrivate ? "user_" + AppContext.CurrentUser.UserData.Id : RandomChatId) + ":" +
                                   FileSpaceName;
                AppContext.CurrentUser.UserData.AttachedFileSpaces =
                    new List<string>(AppContext.CurrentUser.UserData.AttachedFileSpaces).Also(l => l.Add(fileSpaceKey));
                HostScreen.Router.NavigateBack.Execute();
                break;
            default:
                return;
        }

        AppContext.CurrentUser.Save();
    });

    public enum FileSpacePageMode
    {
        Unknown,
        Attach,
        Create
    }
}