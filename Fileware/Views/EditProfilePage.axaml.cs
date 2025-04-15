using Avalonia.ReactiveUI;
using Fileware.ViewModels;
using ReactiveUI;

namespace Fileware.Views;

public partial class EditProfilePage : ReactiveUserControl<EditProfileViewModel>
{
    public EditProfilePage()
    {
        this.WhenActivated(_ => { });
        InitializeComponent();
    }
}