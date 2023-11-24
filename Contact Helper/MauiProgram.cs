using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;

using Syncfusion.Maui.Core.Hosting;
using CommunityToolkit.Maui.Markup;


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
            }).UseMauiCommunityToolkit().UseMauiCommunityToolkitMarkup();

            builder.ConfigureSyncfusionCore();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}