using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Fileware.ViewModels;

namespace Fileware.Views;

public partial class FileSpacePage : ReactiveUserControl<FileSpaceViewModel>
{
    public FileSpacePage()
    {
        InitializeComponent();
    }
}