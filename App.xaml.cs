using TravelSecure.Mobile.Features.Auth.Services;
using TravelSecure.Mobile.Features.Incidents.Services;
using System;

namespace TravelSecure.Mobile
{
    public partial class App : Application
    {
        private readonly AuthService _authService;

        // Exponer IServiceProvider para resolver servicios desde páginas XAML que necesiten constructor sin parámetros
        public static IServiceProvider Services { get; private set; } = default!;

        public App(AuthService authService, IServiceProvider services)
        {
            InitializeComponent();
            _authService = authService;
            Services = services;

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Restaurar sesión si existe token guardado
            await _authService.RestoreSessionAsync();

            // Si está autenticado, ir al Dashboard
            if (await _authService.IsAuthenticatedAsync())
            {
                await Shell.Current.GoToAsync("//main/dashboard");

                // Forzar recarga de alertas en DashboardViewModel en caso de que
                // la vista/VM se haya inicializado antes de restaurar el token.
                try
                {
                    var incidentApi = Services.GetService(typeof(IncidentApiService)) as IncidentApiService;
                    incidentApi?.NotifyDataChanged();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"App OnStart: error notifying IncidentApiService: {ex}");
                }
            }
        }
    }
}
