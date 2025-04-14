using Avalonia.ReactiveUI;
using Fileware.ViewModels;
using ReactiveUI;

namespace Fileware.Views;

public partial class TilesPage : ReactiveUserControl<TilesViewModel>
{
    public TilesPage()
    {
        this.WhenActivated(disposables => { });

        InitializeComponent();
    }
}