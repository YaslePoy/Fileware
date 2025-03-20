﻿using System.Net.Http;
using System.Net.Http.Headers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Fileware.Views;
using Fileware.Windows;

namespace Fileware.Controls;

public partial class MessageBlock : UserControl
{
    public MessageBlock()
    {
        InitializeComponent();
    }

    private void ChangeText(object? sender, RoutedEventArgs e)
    {
        var changeWin = new MessageTextEditWindow
        {
            DataContext = DataContext
        };
        var task = changeWin.ShowDialog<bool>(AppContext.MainWindow);
        task.ContinueWith((t) =>
        {
            if (t.Result)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    var msg = DataContext as Message;
                    msg.OnPropertyChanged("Text");
                    Api.Http.PatchAsync( $"api/Messaging/{msg.Id}",
                        new StringContent("\"" + msg.Text + "\"",
                            MediaTypeWithQualityHeaderValue.Parse("application/json")));
                });
            }
        });
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        var current = DataContext as Message;
        Api.Http.DeleteAsync($"api/Messaging/{current.Id}");
        AppContext.MainWindow.PointsPanel.Children.Remove(this);
    }
}