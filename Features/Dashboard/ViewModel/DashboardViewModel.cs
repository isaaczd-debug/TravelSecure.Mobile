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
    private readonly TravelSecure.Mobile.Features.Incidents.Services.IncidentApiService _incidentApiService;
    private string _searchCity = "Lima";
    private WeatherResponse? _weather;
    private bool _isLoading;
    private bool _isLoadingAlerts;
    private string _errorMessage = string.Empty;
    private ObservableCollection<AlertItem> _recentAlerts = new();
    private int _trafficCount;
    private int _closureCount;
    private int _weatherCount;
    private int _accidentCount;

    public DashboardViewModel(WeatherService weatherService, TravelSecure.Mobile.Features.Incidents.Services.IncidentApiService incidentApiService)
    {
        _weatherService = weatherService;
        _incidentApiService = incidentApiService;

        SearchCommand = new Command(async () => await SearchWeatherAsync());
        NavigateToIncidentsCommand = new Command(async () => await Shell.Current.GoToAsync("//main/incidents/ReportePage"));
        NavigateToAlertsCommand = new Command(async () => await Shell.Current.GoToAsync("//main/alerts/AlertsPage"));
        NavigateToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("//main/profile/ProfilePage"));

        // Cargar datos iniciales (esperar ligeramente para que App.OnStart pueda restaurar sesión)
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await Task.Delay(700);
                await LoadDefaultWeatherAsync();
                await LoadRecentAlertsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Initial load error: {ex}");
            }
        });

        // Suscribirse a cambios (nuevo incidente) para refrescar automáticamente
        try
        {
            _incidentApiService.DataChanged += async (s, e) =>
            {
                try
                {
                    // asegurar ejecución en hilo UI
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await LoadRecentAlertsAsync();
                        await LoadAlertCountersAsync();
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Dashboard DataChanged handler error: {ex}");
                }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard subscribe error: {ex}");
        }
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
        try
        {
            var incidents = (await _incidentApiService.GetRecentIncidentsAsync()).OrderByDescending(i => i.CreatedAt).Take(4).ToList();
            var items = incidents.Select(i => new AlertItem
            {
                Icono = i.Type switch { 1 => "🚗", 2 => "🚧", 3 => "❄️", _ => "🚗" },
                Color = i.Type switch { 1 => "#EF4444", 2 => "#F59E0B", 3 => "#3B82F6", _ => "#6B7280" },
                Tipo = i.Type switch { 1 => "Accidente registrado", 2 => "Cierre de vía", 3 => "Clima extremo", _ => "Tráfico" },
                Lugar = i.Description ?? $"Lat: {i.Latitude:F5}, Lon: {i.Longitude:F5}",
                Tiempo = string.Empty,
                Nivel = i.Type switch { 1 => "ALTO", 2 => "ALTO", 3 => "MEDIO", _ => "BAJO" }
            }).ToArray();

            RecentAlerts = new ObservableCollection<AlertItem>(items);
            // actualizar contadores también cuando cargamos recientes
            await LoadAlertCountersAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadRecentAlertsAsync error: {ex}");
            RecentAlerts = new ObservableCollection<AlertItem>();
        }
        finally
        {
            IsLoadingAlerts = false;
        }
    }

    // Cargar conteos por tipo de incidente
    public async Task LoadAlertCountersAsync()
    {
        try
        {
            var all = (await _incidentApiService.GetAllIncidentsAsync()).ToList();

            // Mapear cada incidente a la misma categoría que AlertsPage (consistencia)
            string MapCategory(int type) => type switch
            {
                1 => "Accidente",
                2 => "Cierre",
                3 => "Clima",
                _ => "Tráfico",
            };

            _accidentCount = all.Count(i => MapCategory(i.Type) == "Accidente");
            _closureCount = all.Count(i => MapCategory(i.Type) == "Cierre");
            _weatherCount = all.Count(i => MapCategory(i.Type) == "Clima");
            _trafficCount = all.Count(i => MapCategory(i.Type) == "Tráfico");

            System.Diagnostics.Debug.WriteLine($"Dashboard: LoadAlertCountersAsync - total={all.Count} traffic={_trafficCount} clima={_weatherCount} accidentes={_accidentCount} cierres={_closureCount}");

            OnPropertyChanged(nameof(TrafficCount));
            OnPropertyChanged(nameof(ClosureCount));
            OnPropertyChanged(nameof(WeatherCount));
            OnPropertyChanged(nameof(AccidentCount));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadAlertCountersAsync error: {ex}");
            _trafficCount = _closureCount = _weatherCount = _accidentCount = 0;
            OnPropertyChanged(nameof(TrafficCount));
            OnPropertyChanged(nameof(ClosureCount));
            OnPropertyChanged(nameof(WeatherCount));
            OnPropertyChanged(nameof(AccidentCount));
        }
    }

    public int TrafficCount => _trafficCount;
    public int ClosureCount => _closureCount;
    public int WeatherCount => _weatherCount;
    public int AccidentCount => _accidentCount;

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