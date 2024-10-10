﻿using System.Net.Http;
using System.Net.Http.Headers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Fileware.Models;
using Fileware.Views;
using Fileware.Windows;

namespace Fileware.Controls;

public partial class FileBlock : UserControl
{
    public FileBlock()
    {
        InitializeComponent();
    }

    private void OnRename(object? sender, RoutedEventArgs e)
    {
        var win = new FileRenameWindow { DataContext = DataContext };
        win.ShowDialog<bool>(MainWindow.Singleton).ContinueWith((t) =>
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
        MainWindow.Singleton.PointsPanel.Children.Remove(this);
    }
}