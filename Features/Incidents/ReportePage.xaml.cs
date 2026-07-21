using TravelSecure.Mobile.Features.Incidents.Services;

namespace TravelSecure.Mobile.Features.Incidents;

public partial class ReportePage : ContentPage
{
    private string _tipoSeleccionado = string.Empty;
    private double _latitud = 0;
    private double _longitud = 0;
    private readonly IncidentApiService _apiService;

    private static readonly Color ColorActivo = Color.FromArgb("#F7CA18");
    private static readonly Color ColorInactivo = Color.FromArgb("#1F2937");

    public ReportePage(IncidentApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    // Constructor sin parámetros para que el XAML loader pueda instanciar la página
    public ReportePage()
        : this(App.Services.GetService(typeof(IncidentApiService)) as IncidentApiService
               ?? throw new InvalidOperationException("IncidentApiService no está registrado en App.Services"))
    {
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ObtenerUbicacion();
    }

    private async Task ObtenerUbicacion()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                LblUbicacion.Text = "Permiso de ubicación denegado";
                LblGpsEstado.TextColor = Color.FromArgb("#EF4444");
                LblGpsEstado.Text = "● SIN GPS";
                return;
            }

            var location = await Geolocation.GetLastKnownLocationAsync();
            location ??= await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location != null)
            {
                _latitud = location.Latitude;
                _longitud = location.Longitude;
                LblUbicacion.Text = $"Lat: {_latitud:F5},  Lon: {_longitud:F5}";
                LblGpsEstado.TextColor = Color.FromArgb("#22C55E");
                LblGpsEstado.Text = "● GPS";
            }
        }
        catch
        {
            LblUbicacion.Text = "No se pudo obtener ubicación";
            LblGpsEstado.TextColor = Color.FromArgb("#EF4444");
            LblGpsEstado.Text = "● ERROR";
        }
    }

    private void OnTipoSelected(object sender, TappedEventArgs e)
    {
        _tipoSeleccionado = e.Parameter?.ToString() ?? string.Empty;

        SetBorder(BtnTrafico, ColorInactivo);
        SetBorder(BtnClima, ColorInactivo);
        SetBorder(BtnAccidente, ColorInactivo);
        SetBorder(BtnCierre, ColorInactivo);

        switch (_tipoSeleccionado)
        {
            case "Tráfico": SetBorder(BtnTrafico, ColorActivo); break;
            case "Clima": SetBorder(BtnClima, ColorActivo); break;
            case "Accidente": SetBorder(BtnAccidente, ColorActivo); break;
            case "Cierre": SetBorder(BtnCierre, ColorActivo); break;
        }

        LblTipoSeleccionado.Text = $"Tipo: {_tipoSeleccionado}";
        CardTipoSeleccionado.IsVisible = true;
    }

    private static void SetBorder(Border b, Color c) => b.Stroke = new SolidColorBrush(c);

    private async void OnEnviarReporte(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_tipoSeleccionado))
        {
            await DisplayAlert("⚠️ Falta el tipo",
                "Por favor selecciona una categoría de incidente antes de enviar.", "OK");
            return;
        }

        BtnEnviar.IsEnabled = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            // Usar el servicio inyectado
            var api = _apiService;

            var tipoEnum = _tipoSeleccionado switch
            {
                "Tráfico" => 0,
                "Accidente" => 1,
                "Cierre" => 2,
                "Clima" => 3,
                _ => 0
            };

            var request = new TravelSecure.Mobile.Features.Incidents.Models.CreateIncidentRequest
            {
                Type = tipoEnum,
                Description = EditorDescripcion.Text,
                Latitude = _latitud,
                Longitude = _longitud
            };

            var response = await api.CreateIncidentAsync(request);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("✅ Alerta enviada",
                    $"Tu reporte de '{_tipoSeleccionado}' fue enviado.\nLos conductores cercanos han sido notificados.", "Aceptar");
                ResetForm();
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                await DisplayAlert("❌ Error al enviar",
                    $"Respuesta del servidor: {response.StatusCode}\n{content}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Error al enviar",
                $"No se pudo enviar el reporte: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            BtnEnviar.IsEnabled = true;
        }
    }

    private void ResetForm()
    {
        _tipoSeleccionado = string.Empty;
        CardTipoSeleccionado.IsVisible = false;
        EditorDescripcion.Text = string.Empty;
        SetBorder(BtnTrafico, ColorInactivo);
        SetBorder(BtnClima, ColorInactivo);
        SetBorder(BtnAccidente, ColorInactivo);
        SetBorder(BtnCierre, ColorInactivo);
    }

    private async void OnNavInicio(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//main/dashboard/DashboardPage");
    private async void OnNavAlertas(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//main/alerts/AlertsPage");
    private async void OnNavPerfil(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//main/profile/ProfilePage");
}
