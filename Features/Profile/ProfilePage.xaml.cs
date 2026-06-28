namespace TravelSecure.Mobile.Features.Profile;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    private async void OnCerrarSesion(object sender, TappedEventArgs e)
    {
        bool confirm = await DisplayAlert("Cerrar Sesión",
            "¿Estás seguro que quieres cerrar tu sesión?",
            "Sí, cerrar sesión", "Cancelar");

        if (confirm)
            await Shell.Current.GoToAsync("//LoginPage");
    }

    private async void OnNavInicio(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//main/dashboard/DashboardPage");
    private async void OnNavReporte(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//main/incidents/ReportePage");
    private async void OnNavAlertas(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//main/alerts/AlertsPage");
}