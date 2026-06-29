using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TravelSecure.Mobile.Features.Auth.Models;
using TravelSecure.Mobile.Features.Auth.Services;
using TravelSecure.Mobile.Helpers;

namespace TravelSecure.Mobile.Features.Auth.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly AuthService? _authService;

        // Propiedades
        private string _name = string.Empty;
        private string _email = string.Empty;
        private string _dni = string.Empty;
        private string _password = string.Empty;
        private string _transportCompany = string.Empty;
        private string _selectedLicense = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _isRegisterEnabled = true;

        // Lista de opciones para el Picker
        public List<string> LicenseOptions { get; } = new()
        {
            "A-I  — Moto / Mototaxi",
            "B-IIb — Auto / Camioneta",
            "A-IIb — Camión Mediano",
            "A-IIIb — Semi-articulado",
            "A-IIIc — Combinado / Convoy"
        };

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Dni
        {
            get => _dni;
            set { _dni = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string TransportCompany
        {
            get => _transportCompany;
            set { _transportCompany = value; OnPropertyChanged(); }
        }

        public string SelectedLicense
        {
            get => _selectedLicense;
            set { _selectedLicense = value; OnPropertyChanged(); }
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

        public bool IsRegisterEnabled
        {
            get => _isRegisterEnabled;
            set { _isRegisterEnabled = value; OnPropertyChanged(); }
        }

        // Comandos
        public ICommand? RegisterCommand { get; }
        public ICommand? GoBackCommand { get; }

        // ✅ CONSTRUCTOR SIN PARÁMETROS (para el diseñador XAML)
        public RegisterViewModel()
        {
            // Constructor vacío para el diseñador
        }

        // ✅ CONSTRUCTOR CON PARÁMETROS (para inyección de dependencias)
        public RegisterViewModel(AuthService authService) : this()
        {
            _authService = authService;

            RegisterCommand = new Command(async () => await ExecuteRegisterAsync());
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async Task ExecuteRegisterAsync()
        {
            if (_authService is null)
            {
                ErrorMessage = "Error de configuración";
                return;
            }

            // Validaciones
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Email) ||
                string.IsNullOrEmpty(Dni) || string.IsNullOrEmpty(Password) ||
                string.IsNullOrEmpty(SelectedLicense))
            {
                ErrorMessage = "Todos los campos son obligatorios";
                return;
            }

            if (Dni.Length != 8 || !Dni.All(char.IsDigit))
            {
                ErrorMessage = "El DNI debe tener exactamente 8 dígitos";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "La contraseña debe tener al menos 6 caracteres";
                return;
            }

            // Obtener valor numérico de la licencia usando Constants
            if (!Constants.LicenseMap.TryGetValue(SelectedLicense, out int licenseValue))
            {
                ErrorMessage = "Selecciona una licencia válida";
                return;
            }

            var request = new RegisterRequest
            {
                Name = Name,
                Email = Email,
                Password = Password,
                Dni = Dni,
                License = licenseValue,
                TransportCompany = string.IsNullOrEmpty(TransportCompany) ? "Sin empresa" : TransportCompany
            };

            try
            {
                IsLoading = true;
                IsRegisterEnabled = false;
                ErrorMessage = string.Empty;

                var response = await _authService.RegisterAsync(request);

                await Shell.Current.DisplayAlert("Éxito", response.Message, "OK");

                // Volver al Login
                await Shell.Current.GoToAsync("..");

            }
            catch (HttpRequestException)
            {
                ErrorMessage = "Error de conexión con el servidor";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message.Contains("email")
                    ? "El email ya está registrado"
                    : "Error al registrar usuario";
            }
            finally
            {
                IsLoading = false;
                IsRegisterEnabled = true;
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}