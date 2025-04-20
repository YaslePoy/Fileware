using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
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

    private void SelectWay(object? sender, TappedEventArgs e)
    {
        foreach (var way in FSWays.Children)
        {
            if (way == sender)
            {
                way.Classes.Add("EdgePressed");
            }
            else
                way.Classes.Remove("EdgePressed");
        }

        foreach (var stages in StagesGrid.Children)
        {
            if (Grid.GetColumn(stages) > 0 )
            {
                stages.IsVisible = false;
            }
        }

        var vm = DataContext as FileSpaceViewModel;
        
        switch ((sender as Control).Tag)
        {
            case "create":
                CreateSpaceForm.IsVisible = true;
                vm.CurrentMode = FileSpaceViewModel.FileSpacePageMode.Create;
                break;
            case "attach":
                AttachSpace.IsVisible = true;
                vm.CurrentMode = FileSpaceViewModel.FileSpacePageMode.Attach;

                break;
        }
    }
}