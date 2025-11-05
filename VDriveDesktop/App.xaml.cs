using VDriveDesktop.Views;

namespace VDriveDesktop
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
