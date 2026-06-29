using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using TravelSecure.Mobile.Features.Auth.Services;

namespace TravelSecure.Mobile.Features.Auth.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService? _authService;

        // Propiedades
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _isLoginEnabled = true;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool IsLoginEnabled
        {
            get => _isLoginEnabled;
            set { _isLoginEnabled = value; OnPropertyChanged(); }
        }

        // Comandos
        public ICommand? LoginCommand { get; }
        public ICommand? GoToRegisterCommand { get; }
        public ICommand? ForgotPasswordCommand { get; }

        // ✅ CONSTRUCTOR SIN PARÁMETROS (para el diseñador XAML)
        public LoginViewModel()
        {
            // Constructor vacío para el diseñador
        }

        // ✅ CONSTRUCTOR CON PARÁMETROS (para inyección de dependencias)
        public LoginViewModel(AuthService authService) : this()
        {
            _authService = authService;

            LoginCommand = new Command(async () => await ExecuteLoginAsync());
            GoToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(RegisterPage)));
            ForgotPasswordCommand = new Command(async () => await OnForgotPassword());
        }

        private async Task ExecuteLoginAsync()
        {
            if (_authService is null)
            {
                ErrorMessage = "Error de configuración";
                return;
            }

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Todos los campos son obligatorios";
                return;
            }

            try
            {
                IsLoading = true;
                IsLoginEnabled = false;
                ErrorMessage = string.Empty;

                var response = await _authService.LoginAsync(Email, Password);

                // ✅ Login exitoso - ir al Dashboard
                await Shell.Current.GoToAsync("//main/dashboard");

            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = "❌ Credenciales inválidas. Verifica tu email y contraseña.";
                }
                else if (ex.StatusCode == HttpStatusCode.BadRequest)
                {
                    ErrorMessage = "❌ Datos de entrada inválidos. Revisa los campos.";
                }
                else if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    ErrorMessage = "❌ Usuario no encontrado. Verifica tu email.";
                }
                else
                {
                    ErrorMessage = "❌ Error de conexión con el servidor. Intenta nuevamente.";
                }
            }
            catch (JsonException)
            {
                ErrorMessage = "❌ Error al procesar la respuesta del servidor.";
            }
            catch (TaskCanceledException)
            {
                ErrorMessage = "❌ Tiempo de espera agotado. Verifica tu conexión.";
            }
            catch (Exception)
            {
                ErrorMessage = "❌ Error inesperado. Intenta nuevamente.";
            }
            finally
            {
                IsLoading = false;
                IsLoginEnabled = true;
            }
        }

        private async Task OnForgotPassword()
        {
            await Shell.Current.DisplayAlert("Recuperar contraseña",
                "Función en desarrollo. Contacta a soporte.", "OK");
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}