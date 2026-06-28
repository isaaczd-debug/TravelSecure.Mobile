namespace TravelSecure.Mobile.Features.Alerts;

public partial class AlertsPage : ContentPage
{
    private readonly List<AlertaItem> _todasAlertas = new()
    {
        new AlertaItem("🚦","EF4444","Tráfico","Congestión severa en Av. Túpac Amaru","A 0.8 km","Hace 2 min","ALTO"),
        new AlertaItem("💥","F59E0B","Accidente","Choque frontal entre 2 vehículos","Carretera Central KM 124","Hace 5 min","ALTO"),
        new AlertaItem("⛈️","3B82F6","Clima","Lluvia intensa con visibilidad reducida","Paso Ticlio – 4,818 msnm","Hace 11 min","MEDIO"),
        new AlertaItem("⛰️","8B5CF6","Accidente","Derrumbe parcial bloquea un carril","Variante de Pasamayo","Hace 22 min","ALTO"),
        new AlertaItem("🚧","F59E0B","Tráfico","Obras de mantenimiento en calzada","Av. Javier Prado Este Cdra 18","Hace 38 min","BAJO"),
        new AlertaItem("❄️","3B82F6","Clima","Granizo intermitente en carretera","Carretera Central – La Oroya","Hace 1 h","MEDIO"),
        new AlertaItem("🚫","EF4444","Cierre","Vía cerrada por manifestación","Av. Abancay, centro de Lima","Hace 1.5 h","ALTO"),
        new AlertaItem("🚦","F59E0B","Tráfico","Semáforos fuera de servicio","Intersección Av. Brasil / Universitaria","Hace 2 h","BAJO"),
    };

    public AlertsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarAlertas(_todasAlertas);
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
        => await CargarAlertas(_todasAlertas);
    private async void OnFiltroTrafico(object sender, TappedEventArgs e)
        => await CargarAlertas(_todasAlertas.Where(a => a.Categoria == "Tráfico"));
    private async void OnFiltroClima(object sender, TappedEventArgs e)
        => await CargarAlertas(_todasAlertas.Where(a => a.Categoria == "Clima"));
    private async void OnFiltroAccidente(object sender, TappedEventArgs e)
        => await CargarAlertas(_todasAlertas.Where(a => a.Categoria == "Accidente"));
    private async void OnFiltroCierre(object sender, TappedEventArgs e)
        => await CargarAlertas(_todasAlertas.Where(a => a.Categoria == "Cierre"));

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
