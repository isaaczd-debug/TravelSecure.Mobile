using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TravelSecure.Mobile.Features.Dashboard.Model;
using TravelSecure.Mobile.Features.Dashboard.Services;

namespace TravelSecure.Mobile.Features.Dashboard.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private readonly WeatherService _weatherService;
    private string _searchCity = "Lima";
    private WeatherResponse? _weather;
    private bool _isLoading;
    private bool _isLoadingAlerts;
    private string _errorMessage = string.Empty;
    private ObservableCollection<AlertItem> _recentAlerts = new();

    public DashboardViewModel(WeatherService weatherService)
    {
        _weatherService = weatherService;

        SearchCommand = new Command(async () => await SearchWeatherAsync());
        NavigateToIncidentsCommand = new Command(async () => await Shell.Current.GoToAsync("//main/incidents/ReportePage"));
        NavigateToAlertsCommand = new Command(async () => await Shell.Current.GoToAsync("//main/alerts/AlertsPage"));
        NavigateToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("//main/profile/ProfilePage"));

        // Cargar datos iniciales en background
        Task.Run(async () =>
        {
            await LoadDefaultWeatherAsync();
            await LoadRecentAlertsAsync();
        });
    }

    // Propiedades...
    public string SearchCity
    {
        get => _searchCity;
        set { _searchCity = value; OnPropertyChanged(); }
    }

    public WeatherResponse? Weather
    {
        get => _weather;
        set { _weather = value; OnPropertyChanged(); OnPropertyChanged(nameof(WeatherStatus)); OnPropertyChanged(nameof(WeatherStatusColor)); OnPropertyChanged(nameof(WeatherEmoji)); OnPropertyChanged(nameof(WeatherLocation)); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public bool IsLoadingAlerts
    {
        get => _isLoadingAlerts;
        set { _isLoadingAlerts = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public ObservableCollection<AlertItem> RecentAlerts
    {
        get => _recentAlerts;
        set { _recentAlerts = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasAlerts)); OnPropertyChanged(nameof(HasNoAlerts)); }
    }

    public bool HasAlerts => RecentAlerts?.Any() == true;
    public bool HasNoAlerts => !HasAlerts;

    // Propiedades calculadas
    public string WeatherStatus
    {
        get
        {
            if (Weather == null) return "● Verificando";
            var desc = Weather.Description?.ToLower() ?? "";
            return desc.Contains("lluvia") || desc.Contains("tormenta") || desc.Contains("niebla")
                ? "⚠️ Precaución en ruta"
                : "✅ Ruta Segura";
        }
    }

    public Color WeatherStatusColor
    {
        get
        {
            if (Weather == null) return Color.FromArgb("#6B7280");
            var desc = Weather.Description?.ToLower() ?? "";
            return desc.Contains("lluvia") || desc.Contains("tormenta") || desc.Contains("niebla")
                ? Color.FromArgb("#F59E0B")
                : Color.FromArgb("#22C55E");
        }
    }

    public string WeatherEmoji
    {
        get
        {
            if (Weather == null) return "☁️";
            var desc = Weather.Description?.ToLower() ?? "";
            if (desc.Contains("lluvia")) return "🌧️";
            if (desc.Contains("nublado")) return "☁️";
            if (desc.Contains("claro") || desc.Contains("soleado")) return "☀️";
            if (desc.Contains("niebla")) return "🌫️";
            if (desc.Contains("tormenta")) return "⛈️";
            return "🌤️";
        }
    }

    public string WeatherLocation => Weather != null ? $"{Weather.Name}, {Weather.Country}" : "Lima, PE";

    // Commands
    public ICommand SearchCommand { get; }
    public ICommand NavigateToIncidentsCommand { get; }
    public ICommand NavigateToAlertsCommand { get; }
    public ICommand NavigateToProfileCommand { get; }

    // Métodos
    private async Task LoadDefaultWeatherAsync()
    {
        await SearchWeatherAsync("Lima");
    }

    public async Task SearchWeatherAsync(string? city = null)
    {
        var cityToSearch = city ?? SearchCity;

        if (string.IsNullOrWhiteSpace(cityToSearch))
        {
            ErrorMessage = "Por favor ingrese una ciudad";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            Weather = await _weatherService.GetWeatherAsync(cityToSearch);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            ErrorMessage = "Sesión expirada. Por favor inicie sesión nuevamente";
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Error de conexión con el servidor";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadRecentAlertsAsync()
    {
        IsLoadingAlerts = true;
        await Task.Delay(900);

        var alerts = new[]
        {
            new AlertItem { Icono = "🚗", Color = "#EF4444", Tipo = "Accidente registrado", Lugar = "Carretera Central, KM 124", Tiempo = "Hace 3 min", Nivel = "ALTO" },
            new AlertItem { Icono = "❄️", Color = "#3B82F6", Tipo = "Clima extremo", Lugar = "Paso Ticlio – Carretera Central", Tiempo = "Hace 11 min", Nivel = "MEDIO" },
            new AlertItem { Icono = "🚧", Color = "#F59E0B", Tipo = "Obras en la vía", Lugar = "Av. Javier Prado Este, cuadra 18", Tiempo = "Hace 38 min", Nivel = "BAJO" },
            new AlertItem { Icono = "⛰️", Color = "#8B5CF6", Tipo = "Derrumbe parcial", Lugar = "Variante de Pasamayo", Tiempo = "Hace 1 h", Nivel = "ALTO" },
        };

        RecentAlerts = new ObservableCollection<AlertItem>(alerts);
        IsLoadingAlerts = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class AlertItem
{
    public string Icono { get; set; } = string.Empty;
    public string Color { get; set; } = "#6B7280";
    public string Tipo { get; set; } = string.Empty;
    public string Lugar { get; set; } = string.Empty;
    public string Tiempo { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
}