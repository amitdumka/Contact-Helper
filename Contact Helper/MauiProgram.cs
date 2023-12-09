using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Contact_Helper.VCF;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using UXDivers.Grial;

namespace Contact_Helper
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().UseMauiCommunityToolkitCore().ConfigureFonts(fonts =>
                {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("InputLayoutIcons.ttf", "InputLayoutIcons");
                fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
                    fonts.AddFont("Poppins-Regular.ttf","Poppins");
                })
.UseMauiCommunityToolkit()
                .UseGrial();

            builder.ConfigureSyncfusionCore();
           // builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<AppContext>();
            builder.Services.AddTransient<ContactPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}