using Avalonia.Controls;
using Avalonia.Interactivity;
using Fileware.Models;
using Fileware.ViewModels;

namespace Fileware.Controls;

public partial class TagPoint : UserControl
{
    public TagPoint()
    {
        InitializeComponent();
    }

    private void RemoveTag(object? sender, RoutedEventArgs e)
    {
        
        ((Parent as WrapPanel).DataContext as ITagContainer).Tags.Remove(DataContext as Tag);
        ((Parent as WrapPanel).DataContext as ITagContainer).UpdateTagPanel();
    }
}