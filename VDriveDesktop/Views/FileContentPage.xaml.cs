using VDriveDesktop.ViewModels;

namespace VDriveDesktop.Views;

public partial class FileContentPage : ContentPage
{
    public FileContentPage(FileContentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}