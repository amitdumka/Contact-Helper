﻿using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Core;

using Syncfusion.Maui.Core.Hosting;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui;

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
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<DBClass>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}