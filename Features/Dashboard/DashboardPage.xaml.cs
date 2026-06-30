using System.Globalization;
using System.Text.Json;
using TravelSecure.Mobile.Features.Auth.Service;

namespace TravelSecure.Mobile.Features.Dashboard;

public partial class DashboardPage : ContentPage
{


    //integracion de la api
    private const string ApiUrlBase = "http://localhost:5138";
    private readonly WeatherService _weatherService;

    private readonly HttpClient _httpClient;
    private bool _isCargandoAlertas = false;

    public DashboardPage()
    {
        InitializeComponent();
        //Inicializacion consumo de api autenticado
        _weatherService = new WeatherService();

        //Centramos el bypass
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(8) };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarClimaInicial();
        await CargarAlertasRecientes();
    }


    private async Task CargarClimaInicial()
    {
        try
        {
            // Intentamos obtener la ubicación, si no se puede, usamos Lima por defecto
            string ciudadPorDefecto = "Lima";
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location != null)
                {
                    ciudadPorDefecto = "Lima";
                }
            }
            catch { }

            var url = $"{ApiUrlBase}/api/WeatherForecast/ruta/{ciudadPorDefecto}";
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonDocument.Parse(response);

            var root = data.RootElement;
            var temp = root.GetProperty("temp").GetDecimal();
            var desc = root.GetProperty("description").GetString() ?? "despejado";
            var name = root.GetProperty("name").GetString();
            var country = root.GetProperty("country").GetString();
            var feelsLike = root.GetProperty("feelsLike").GetDecimal();
            var humidity = data.RootElement.GetProperty("humidity").GetInt32();
            var wind = data.RootElement.GetProperty("windSpeed").GetDecimal();

            LblTempClima.Text = $"{temp:F0}°";
            LblDescClima.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(desc);


            LblTemp.Text = $"{temp:F0}°";
            LblDesc.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(desc);
            LblCiudad.Text = $"{name}, {country}";
            LblFeelsLike.Text = $"Sensación térmica: {feelsLike:F0}°";
            LblHumidity.Text = $"Humedad: {humidity}%";
            LblWind.Text = $"Viento: {wind} m/s";

            var cond = desc.ToLower();
            if (cond.Contains("lluvia") || cond.Contains("tormenta") || cond.Contains("niebla"))
            {
                LblEstadoRuta.Text = "⚠️ Precaución en ruta";
                LblEstadoRuta.TextColor = Color.FromArgb("#F59E0B");
            }
            else
            {
                LblEstadoRuta.Text = "✅ Ruta Segura";
                LblEstadoRuta.TextColor = Color.FromArgb("#22C55E");
            }
        }
        catch
        {
            //Vlores por defecto
            LblTempClima.Text = "19°";
            LblDescClima.Text = "Parcialmente nublado";
            LblEstadoRuta.Text = "✅ Ruta Segura";
            LblEstadoRuta.TextColor = Color.FromArgb("#22C55E");
        }
    }





    private async void OnBtnBuscarClima(object sender, EventArgs e)
    {
        string ciudad = TxtRutaCiudad.Text?.Trim();

        if (string.IsNullOrWhiteSpace(ciudad))
        {
            await DisplayAlert("Atención", "Por favor ingrese el nombre de una ciudad", "OK");
            return;
        }

        try
        {
            string token = await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Sesión Expirada", "No se encontró un token válido. Por favor, vuelve a iniciar sesión.", "OK");
                return;
            }

            var weatherService = new WeatherService();

            var resultado = await weatherService.GetWeatherAsync(ciudad,token);


            if (resultado != null)
            {
                // 3. Muestra de datos corregidos con el nuevo modelo en Mayúsculas
                var temp = resultado.temp;
                var desc = resultado.description ?? "despejado";
                var name = resultado.name;
                var country = resultado.country;
                var feelsLike = resultado.feelsLike;
                var humidity = resultado.humidity;
                var wind = resultado.windSpeed;

                // Sincronizacion de interfaz gráfica (El resto de tu código queda exactamente igual)
                LblTempClima.Text = $"{temp:F0}°";
                LblDescClima.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(desc);

                LblTemp.Text = $"{temp:F0}°";
                LblDesc.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(desc);

                if (LblCiudad != null) LblCiudad.Text = $"{name}, {country}";
                if (LblFeelsLike != null) LblFeelsLike.Text = $"Sensación térmica: {feelsLike:F0}°";
                if (LblHumidity != null) LblHumidity.Text = $"Humedad: {humidity}%";
                if (LblWind != null) LblWind.Text = $"Viento: {wind} m/s";

                var cond = desc.ToLower();
                if (cond.Contains("lluvia") || cond.Contains("tormenta") || cond.Contains("niebla"))
                {
                    LblEstadoRuta.Text = "⚠️ Precaución en ruta";
                    LblEstadoRuta.TextColor = Color.FromArgb("#F59E0B");
                }
                else
                {
                    LblEstadoRuta.Text = "✅ Ruta Segura";
                    LblEstadoRuta.TextColor = Color.FromArgb("#22C55E");
                }
            }
            else
            {
                await DisplayAlert("Error", "No se pudo obtener el clima. Verifica la ciudad o tu sesión.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo obtener el clima: {ex.Message}", "OK");
        }    
    }

    private async Task CargarAlertasRecientes()
    {
        //Evitamos duplicados
        ListaAlertas.Clear();

        await Task.Delay(900);

        LoadingAlertas.IsRunning = false;
        LoadingAlertas.IsVisible = false;

        var alertas = new[]
        {
            new { Icono = "🚗", Color = "#EF4444", Tipo = "Accidente registrado", Lugar = "Carretera Central, KM 124", Tiempo = "Hace 3 min", Nivel = "ALTO" },
            new { Icono = "❄️", Color = "#3B82F6", Tipo = "Clima extremo", Lugar = "Paso Ticlio – Carretera Central", Tiempo = "Hace 11 min", Nivel = "MEDIO" },
            new { Icono = "🚧", Color = "#F59E0B", Tipo = "Obras en la vía", Lugar = "Av. Javier Prado Este, cuadra 18", Tiempo = "Hace 38 min", Nivel = "BAJO" },
            new { Icono = "⛰️", Color = "#8B5CF6", Tipo = "Derrumbe parcial", Lugar = "Variante de Pasamayo", Tiempo = "Hace 1 h", Nivel = "ALTO" },
        };

        foreach (var alerta in alertas)
        {
            var fila = new Border
            {
                BackgroundColor = Color.FromArgb("#111827"),
                StrokeThickness = 0,
                Padding = new Thickness(16, 12),
                Margin = new Thickness(4, 2),
            };
            fila.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = 44 },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var iconoBorder = new Border
            {
                BackgroundColor = Color.FromArgb(alerta.Color + "22"),
                WidthRequest = 38,
                HeightRequest = 38,
                StrokeThickness = 0,
                VerticalOptions = LayoutOptions.Center
            };
            iconoBorder.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) };
            iconoBorder.Content = new Label { Text = alerta.Icono, FontSize = 18, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };

            var info = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
            info.Add(new Label { Text = alerta.Tipo, FontSize = 13, TextColor = Colors.White, FontAttributes = FontAttributes.Bold });
            info.Add(new Label { Text = alerta.Lugar, FontSize = 11, TextColor = Color.FromArgb("#6B7280") });

            var right = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(alerta.Color + "22"),
                Padding = new Thickness(6, 2),
                StrokeThickness = 0
            };
            badge.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(4) };
            badge.Content = new Label { Text = alerta.Nivel, FontSize = 9, TextColor = Color.FromArgb(alerta.Color), FontAttributes = FontAttributes.Bold };

            right.Add(badge);
            right.Add(new Label { Text = alerta.Tiempo, FontSize = 10, TextColor = Color.FromArgb("#374151"), HorizontalOptions = LayoutOptions.End });

            Grid.SetColumn(iconoBorder, 0);
            Grid.SetColumn(info, 1);
            Grid.SetColumn(right, 2);
            grid.Add(iconoBorder);
            grid.Add(info);
            grid.Add(right);

            fila.Content = grid;
            ListaAlertas.Add(fila);
        }
    }

    private async void OnBuscarRuta(object sender, EventArgs e)
    {
        await DisplayAlert("Próximamente 🗺️", "La búsqueda de rutas estará disponible en el siguiente avance.", "Entendido");
    }

    private async void OnNavReporte(object sender, EventArgs e) => await Shell.Current.GoToAsync("//main/incidents/ReportePage");
    private async void OnNavAlertas(object sender, EventArgs e) => await Shell.Current.GoToAsync("//main/alerts/AlertsPage");
    private async void OnNavPerfil(object sender, EventArgs e) => await Shell.Current.GoToAsync("//main/profile/ProfilePage");
}