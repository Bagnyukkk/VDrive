using VDriveDesktop.ViewModels;

namespace VDriveDesktop.Views;

public partial class FilesPage : ContentPage
{
    public FilesPage(FilesViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FilesViewModel viewModel)
        {
            viewModel.LoadFilesCommand.Execute(null);
        }
    }
}