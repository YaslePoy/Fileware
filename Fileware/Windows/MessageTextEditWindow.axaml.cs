using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Fileware.Windows;

public partial class MessageTextEditWindow : Window
{
    public MessageTextEditWindow()
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