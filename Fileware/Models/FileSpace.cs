using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;

namespace Fileware.Models;

public class FileSpace
{
    public string Id { get; set; }

    public string Display
    {
        get
        {
            var extracted = Id[(Id.IndexOf(':') + 1)..];

            if (extracted == "master") extracted = "Мастер пространство";

            return extracted;
        }
    }

    public ICommand Exit => ReactiveCommand.Create(() =>
    {
        AppContext.CurrentUser!.UserData.AttachedFileSpaces = new List<string>(AppContext.CurrentUser.UserData
            .AttachedFileSpaces).Also(l => l.Remove(Id));
        AppContext.CurrentUser.Save();
    });

    public ICommand Share => ReactiveCommand.Create(() =>
    {
        TopLevel.GetTopLevel(AppContext.WindowInstance)!.Clipboard!.SetTextAsync(
            Convert.ToBase64String(Encoding.Default.GetBytes(Id)));
    });

    public ContextMenu Menu
    {
        get
        {
            var menu = new ContextMenu();
            if (Id.EndsWith(":master")) return menu;

            if (!Id.StartsWith("user_"))
            {
                var share = new MenuItem { Header = "Поделиться", Command = Share };
                menu.Items.Add(share);
            }

            menu.Items.Add(new MenuItem
            {
                Header = "Выйти", Command = Exit
            });
            return menu;
        }
    }

    public static FileSpace OfUser(CommonUserData commonUser)
    {
        return new FileSpace { Id = $"user_{commonUser.Id}:master" };
    }
}