using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using DynamicData;
using ReactiveUI;

namespace Fileware.Models;

public class FileSpace
{
    public string Id { get; set; }

    public string Display
    {
        get
        {
            var extracted = Id[(Id.IndexOf(':')+1)..];
            
            if (extracted == "master")
            {
                extracted = "Мастер пространство";
            }

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
        TopLevel.GetTopLevel(AppContext.WindowInstance)!.Clipboard!.SetTextAsync(Convert.ToBase64String(Encoding.Default.GetBytes(this.Id)));
    });

    public ContextMenu Menu
    {
        get
        {
            var menu = new ContextMenu();
            if (this.Id.EndsWith(":master"))
            {
                return menu;
            }

            if (!this.Id.StartsWith("user_"))
            {
                var share = new MenuItem { Header = "Поделиться", Command = this.Share };
                menu.Items.Add(share);
            }

            menu.Items.Add(new MenuItem {
                Header = "Выйти", Command = this.Exit
            });
            return menu;
        }
    }

    public static FileSpace OfUser(UserData user) => new() { Id = $"user_{user.Id}:master" };
}