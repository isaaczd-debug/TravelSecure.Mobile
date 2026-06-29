using TravelSecure.Mobile.Features.Auth.Services;

namespace TravelSecure.Mobile
{
    public partial class App : Application
    {
        private readonly AuthService _authService;

        public App(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;

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
            }
        }
    }
}