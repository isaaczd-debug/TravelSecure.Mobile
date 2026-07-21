namespace TravelSecure.Mobile.Features.Alerts;

public partial class AlertsPage : ContentPage
{
    private readonly List<AlertaItem> _todasAlertas = new();
    private CancellationTokenSource? _sseCts;
    private readonly TravelSecure.Mobile.Features.Incidents.Services.IncidentApiService _incidentApiService;
    private List<AlertaItem> _currentLoaded = new();

    public AlertsPage()
    {
        InitializeComponent();
        _incidentApiService = App.Services.GetService(typeof(TravelSecure.Mobile.Features.Incidents.Services.IncidentApiService)) as TravelSecure.Mobile.Features.Incidents.Services.IncidentApiService
            ?? throw new InvalidOperationException("IncidentApiService no está registrado");
        // Inicializar estado de filtros (Todas activa)
        HighlightFilter(null);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAlertsFromApiAsync();

        // Iniciar SSE para recibir nuevos incidentes en tiempo real (si el backend lo soporta)
        _sseCts = new CancellationTokenSource();
        _ = Task.Run(() => StartSseListening(_sseCts.Token));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _sseCts?.Cancel();
    }

    private async Task CargarAlertas(IEnumerable<AlertaItem> alertas)
    {
        LoadingAlertas.IsRunning = true;
        LoadingAlertas.IsVisible = true;

        var items = ListaAlertas.Children.ToList();
        foreach (var item in items)
            if (item != LoadingAlertas)
                ListaAlertas.Remove(item);

        await Task.Delay(600);

        LoadingAlertas.IsRunning = false;
        LoadingAlertas.IsVisible = false;

        ListaAlertas.Add(new Label
        {
            Text = "HOY",
            FontSize = 10,
            TextColor = Color.FromArgb("#6B7280"),
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 8, 0, 4),
            CharacterSpacing = 2
        });

        foreach (var a in alertas)
            ListaAlertas.Add(CrearCard(a));
    }

    private async Task LoadAlertsFromApiAsync()
    {
        LoadingAlertas.IsRunning = true;
        LoadingAlertas.IsVisible = true;

        var items = ListaAlertas.Children.ToList();
        foreach (var item in items)
            if (item != LoadingAlertas)
                ListaAlertas.Remove(item);

        try
        {
            // Obtener ubicación del usuario (para calcular distancia)
            double? userLat = null, userLon = null;
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status == PermissionStatus.Granted)
                {
                    var loc = await Geolocation.GetLastKnownLocationAsync();
                    if (loc == null)
                        loc = await Geolocation.GetLocationAsync(new GeolocationRequest { DesiredAccuracy = GeolocationAccuracy.Medium, Timeout = TimeSpan.FromSeconds(5) });
                    if (loc != null)
                    {
                        userLat = loc.Latitude; userLon = loc.Longitude;
                    }
                }
            }
            catch { /* ignorar permiso/errores, seguiremos sin distancia */ }

            // Llamada al backend
            var incidents = await _incidentApiService.GetAllIncidentsAsync();
            _currentLoaded.Clear();

            // Ordenar por CreatedAt desc
            var ordered = incidents.OrderByDescending(i => i.CreatedAt).ToList();

            // Header
            ListaAlertas.Add(new Label
            {
                Text = "HOY",
                FontSize = 10,
                TextColor = Color.FromArgb("#6B7280"),
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 8, 0, 4),
                CharacterSpacing = 2
            });

            foreach (var inc in ordered)
            {
                // Geocodificar a ubicación legible
                string lugar = string.Empty;
                try
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(inc.Latitude, inc.Longitude);
                    var p = placemarks?.FirstOrDefault();
                    if (p != null)
                    {
                        var parts = new List<string>();
                        if (!string.IsNullOrEmpty(p.Locality)) parts.Add(p.Locality);
                        if (!string.IsNullOrEmpty(p.SubAdminArea)) parts.Add(p.SubAdminArea);
                        if (!string.IsNullOrEmpty(p.AdminArea)) parts.Add(p.AdminArea);
                        if (!string.IsNullOrEmpty(p.CountryName)) parts.Add(p.CountryName);
                        lugar = parts.Count > 0 ? string.Join(", ", parts) : string.Empty;
                    }
                }
                catch { /* ignorar geocoding errors */ }

                if (string.IsNullOrEmpty(lugar))
                    lugar = $"Lat: {inc.Latitude:F5}, Lon: {inc.Longitude:F5}";

                // Distancia
                string distanciaText = string.Empty;
                if (userLat.HasValue && userLon.HasValue)
                {
                    var km = HaversineDistance(userLat.Value, userLon.Value, inc.Latitude, inc.Longitude);
                    distanciaText = $"A {km:F1} km de tu ubicación";
                }

                var (icono, color, tipoLabel, nivel) = MapIncidentType(inc.Type);

                var categoriaShort = inc.Type switch { 1 => "Accidente", 2 => "Cierre", 3 => "Clima", _ => "Tráfico" };
                var titulo = !string.IsNullOrEmpty(inc.Description) ? inc.Description : tipoLabel;

                var lugarDisplay = lugar + (string.IsNullOrEmpty(distanciaText) ? string.Empty : " • " + distanciaText);

                var alerta = new AlertaItem(icono, color.Replace("#", ""), categoriaShort, titulo, lugarDisplay, string.Empty, nivel);
                ListaAlertas.Add(CrearCard(alerta));
                _currentLoaded.Add(alerta);
            }
            // actualizar contador
            UpdateBadgeCount(_currentLoaded.Count);
        }
        catch (Exception ex)
        {
            // mostrar mínimo si hay error
            ListaAlertas.Add(new Label { Text = "No se pudieron cargar las alertas", TextColor = Color.FromArgb("#EF4444"), HorizontalOptions = LayoutOptions.Center });
            System.Diagnostics.Debug.WriteLine($"LoadAlertsFromApiAsync error: {ex}");
        }
        finally
        {
            LoadingAlertas.IsRunning = false;
            LoadingAlertas.IsVisible = false;
        }
    }

    private void UpdateBadgeCount(int count)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LblBadgeCount.Text = $"{count} ACTIVAS";
        });
    }

    private static (string icono, string color, string tipo, string nivel) MapIncidentType(int type)
    {
        // Mapeo consistente con Dashboard
        return type switch
        {
            1 => ("🚗", "#EF4444", "Accidente registrado", "ALTO"),
            2 => ("🚧", "#F59E0B", "Cierre de vía", "ALTO"),
            3 => ("❄️", "#3B82F6", "Clima extremo", "MEDIO"),
            _ => ("🚦", "#6B7280", "Tráfico", "BAJO"),
        };
    }

    private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371; // km
        double dLat = ToRad(lat2 - lat1);
        double dLon = ToRad(lon2 - lon1);
        double a = Math.Sin(dLat/2) * Math.Sin(dLat/2) + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) * Math.Sin(dLon/2) * Math.Sin(dLon/2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
        return R * c;
    }

    private static double ToRad(double deg) => deg * (Math.PI/180);

    private async Task StartSseListening(CancellationToken ct)
    {
        try
        {
            await _incidentApiService.StartSseAsync(async incident =>
            {
                // Al llegar un nuevo incidente, añadir al inicio de la lista
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var (icono, color, tipo, nivel) = MapIncidentType(incident.Type);
                    var lugar = $"Lat: {incident.Latitude:F5}, Lon: {incident.Longitude:F5}";
                    var titulo = !string.IsNullOrEmpty(incident.Description) ? incident.Description : tipo;
                    var alerta = new AlertaItem(icono, color.Replace("#", ""), tipo, titulo, lugar, string.Empty, nivel);
                    ListaAlertas.Insert(ListaAlertas.IndexOf(LoadingAlertas) + 1, CrearCard(alerta));
                });
            }, ct);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartSseListening error: {ex}");
        }
    }

    private static View CrearCard(AlertaItem a)
    {
        var card = new Border
        {
            BackgroundColor = Color.FromArgb("#111827"),
            StrokeThickness = 1,
            Stroke = new SolidColorBrush(Color.FromArgb("#1F2937")),
            Padding = new Thickness(14, 12),
            Margin = new Thickness(0, 0, 0, 2)
        };
        card.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
        { CornerRadius = new CornerRadius(14) };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = 48 },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        // Icono
        var iconoBox = new Border
        {
            BackgroundColor = Color.FromArgb("#" + a.Color + "22"),
            WidthRequest = 40,
            HeightRequest = 40,
            StrokeThickness = 0,
            VerticalOptions = LayoutOptions.Center
        };
        iconoBox.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
        { CornerRadius = new CornerRadius(12) };
        iconoBox.Content = new Label
        {
            Text = a.Icono,
            FontSize = 20,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Texto
        var info = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        info.Add(new Label { Text = a.Titulo, FontSize = 13, TextColor = Colors.White, FontAttributes = FontAttributes.Bold });
        info.Add(new Label { Text = a.Lugar, FontSize = 11, TextColor = Color.FromArgb("#6B7280") });

        var badgeTipo = new Border
        {
            BackgroundColor = Color.FromArgb("#" + a.Color + "18"),
            Padding = new Thickness(6, 2),
            StrokeThickness = 0,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 4, 0, 0)
        };
        badgeTipo.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
        { CornerRadius = new CornerRadius(4) };
        badgeTipo.Content = new Label
        {
            Text = a.Categoria.ToUpper(),
            FontSize = 9,
            TextColor = Color.FromArgb("#" + a.Color),
            FontAttributes = FontAttributes.Bold
        };
        info.Add(badgeTipo);

        // Derecha
        var nivelColor = a.Nivel == "ALTO" ? "#EF4444" : a.Nivel == "MEDIO" ? "#F59E0B" : "#22C55E";
        var right = new VerticalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            MinimumWidthRequest = 60
        };
        right.Add(new Label
        {
            Text = "● " + a.Nivel,
            FontSize = 11,
            TextColor = Color.FromArgb(nivelColor),
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.End
        });
        right.Add(new Label
        {
            Text = a.Tiempo,
            FontSize = 10,
            TextColor = Color.FromArgb("#374151"),
            HorizontalOptions = LayoutOptions.End
        });

        Grid.SetColumn(iconoBox, 0);
        Grid.SetColumn(info, 1);
        Grid.SetColumn(right, 2);
        grid.Add(iconoBox);
        grid.Add(info);
        grid.Add(right);

        card.Content = grid;
        return card;
    }

    // FILTROS
    private async void OnFiltroTodas(object sender, TappedEventArgs e)
        => await ApplyFilterAsync(null);
    private async void OnFiltroTrafico(object sender, TappedEventArgs e)
        => await ApplyFilterAsync("Tráfico");
    private async void OnFiltroClima(object sender, TappedEventArgs e)
        => await ApplyFilterAsync("Clima");
    private async void OnFiltroAccidente(object sender, TappedEventArgs e)
        => await ApplyFilterAsync("Accidente");
    private async void OnFiltroCierre(object sender, TappedEventArgs e)
        => await ApplyFilterAsync("Cierre");

    private async Task ApplyFilterAsync(string? categoria)
    {
        IEnumerable<AlertaItem> source = _currentLoaded.Any() ? _currentLoaded : _todasAlertas;
        IEnumerable<AlertaItem> filtered = categoria == null ? source : source.Where(a => a.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));
        await CargarAlertas(filtered);
        UpdateBadgeCount(filtered.Count());
        HighlightFilter(categoria);
    }

    // Resalta el filtro activo cambiando background/stroke de los Borders
    private void HighlightFilter(string? categoria)
    {
        try
        {
            // Reset
            FiltroTodas.BackgroundColor = Color.FromArgb("#F7CA18");
            FiltroTrafico.BackgroundColor = Color.FromArgb("#111827");
            FiltroClima.BackgroundColor = Color.FromArgb("#111827");
            FiltroAccidente.BackgroundColor = Color.FromArgb("#111827");
            FiltroCierre.BackgroundColor = Color.FromArgb("#111827");

            FiltroTodas.Stroke = new SolidColorBrush(Color.FromArgb("#1F2937"));
            FiltroTrafico.Stroke = new SolidColorBrush(Color.FromArgb("#1F2937"));
            FiltroClima.Stroke = new SolidColorBrush(Color.FromArgb("#1F2937"));
            FiltroAccidente.Stroke = new SolidColorBrush(Color.FromArgb("#1F2937"));
            FiltroCierre.Stroke = new SolidColorBrush(Color.FromArgb("#1F2937"));

            // Aplicar estilo activo
            if (categoria == null)
            {
                // Todas
                FiltroTodas.BackgroundColor = Color.FromArgb("#F7E5A0");
                FiltroTodas.Stroke = new SolidColorBrush(Color.FromArgb("#F7CA18"));
            }
            else if (categoria.Equals("Tráfico", StringComparison.OrdinalIgnoreCase))
            {
                FiltroTrafico.BackgroundColor = Color.FromArgb("#0F1D35");
                FiltroTrafico.Stroke = new SolidColorBrush(Color.FromArgb("#F7CA18"));
            }
            else if (categoria.Equals("Clima", StringComparison.OrdinalIgnoreCase))
            {
                FiltroClima.BackgroundColor = Color.FromArgb("#0F1D35");
                FiltroClima.Stroke = new SolidColorBrush(Color.FromArgb("#3B82F6"));
            }
            else if (categoria.Equals("Accidente", StringComparison.OrdinalIgnoreCase))
            {
                FiltroAccidente.BackgroundColor = Color.FromArgb("#0F1D35");
                FiltroAccidente.Stroke = new SolidColorBrush(Color.FromArgb("#EF4444"));
            }
            else if (categoria.Equals("Cierre", StringComparison.OrdinalIgnoreCase))
            {
                FiltroCierre.BackgroundColor = Color.FromArgb("#0F1D35");
                FiltroCierre.Stroke = new SolidColorBrush(Color.FromArgb("#F59E0B"));
            }
        }
        catch { /* ignorar si elementos no están inicializados aún */ }
    }

    // NAV BAR
    private async void OnNavInicio(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//main/dashboard/DashboardPage");
    private async void OnNavReporte(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//main/incidents/ReportePage");
    private async void OnNavPerfil(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//main/profile/ProfilePage");
}

// MODELO — 7 parámetros 
public class AlertaItem
{
    public string Icono { get; }
    public string Color { get; }
    public string Categoria { get; }
    public string Titulo { get; }
    public string Lugar { get; }
    public string Tiempo { get; }
    public string Nivel { get; }

    public AlertaItem(string icono, string color, string categoria,
                      string titulo, string lugar, string tiempo, string nivel)
    {
        Icono = icono;
        Color = color;
        Categoria = categoria;
        Titulo = titulo;
        Lugar = lugar;
        Tiempo = tiempo;
        Nivel = nivel;
    }
}
