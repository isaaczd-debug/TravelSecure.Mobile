using TravelSecure.Mobile.Features.Auth;
using TravelSecure.Mobile.Features.Incidents;
using TravelSecure.Mobile.Features.Alerts;
using TravelSecure.Mobile.Features.Profile;

namespace TravelSecure.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rutas adicionales accesibles por navegación push
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));

        Routing.RegisterRoute(nameof(ReportePage), typeof(ReportePage));

        Routing.RegisterRoute(nameof(AlertsPage), typeof(AlertsPage));

        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
    }
}