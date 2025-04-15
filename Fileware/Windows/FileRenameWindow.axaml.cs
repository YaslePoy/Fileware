﻿using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Fileware.Windows;

public partial class FileRenameWindow : Window
{
    public FileRenameWindow()
    {
        InitializeComponent();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void OnApply(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
}