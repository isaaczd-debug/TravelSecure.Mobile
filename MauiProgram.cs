using Microsoft.Extensions.Logging;
using TravelSecure.Mobile.Features.Auth;
using TravelSecure.Mobile.Features.Auth.Services;
using TravelSecure.Mobile.Features.Auth.ViewModels;
using TravelSecure.Mobile.Features.Dashboard;
using TravelSecure.Mobile.Features.Dashboard.Services;
using TravelSecure.Mobile.Features.Dashboard.ViewModels;

namespace TravelSecure.Mobile
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

            // 🔥 REGISTRO DE SERVICIOS (INYECCIÓN DE DEPENDENCIAS)
            builder.Services.AddSingleton<TokenStorage>();
            builder.Services.AddSingleton<AuthService>();

            // 🔥 REGISTRO DE VIEWMODELS
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();

            // 🔥 REGISTRO DE PAGES
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();


            builder.Services.AddSingleton<WeatherService>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<DashboardPage>();

            return builder.Build();
        }
    }
}