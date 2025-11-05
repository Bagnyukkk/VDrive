using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using VDriveDesktop.Services;
using VDriveDesktop.ViewModels;
using VDriveDesktop.Views;

namespace VDriveDesktop
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddTransient<FilesViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();

            builder.Services.AddSingleton<FilesPage>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();

            builder.Services.AddTransient<VDriveDesktop.ViewModels.FileContentViewModel>();
            builder.Services.AddTransient<VDriveDesktop.Views.FileContentPage>();

            builder.UseMauiApp<App>().UseMauiCommunityToolkit();

            return builder.Build();
        }
    }
}
