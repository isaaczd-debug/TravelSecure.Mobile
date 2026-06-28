using TravelSecure.Mobile.Features.Dashboard;


namespace TravelSecure.Mobile.Features.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryEmail.Text) ||
            string.IsNullOrWhiteSpace(EntryPassword.Text))
        {
            await Shell.Current.DisplayAlert("Campo vacío",
                "Por favor ingresa tu correo y contraseña.", "OK");
            return;
        }

        BtnLogin.IsEnabled = false;

        LoadingIndicator.IsVisible = true;

        LoadingIndicator.IsRunning = true;

        await Task.Delay(1500);

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        BtnLogin.IsEnabled = true;

        // Navegar al TabBar principal
        await Shell.Current.GoToAsync("//main/dashboard/DashboardPage");
    }

    private async void OnGoToRegister(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    private async void OnForgotPassword(object sender, EventArgs e)
    {
        await DisplayAlert("Recuperar contraseña",
            "Contacta a soporte@travelsecure.pe\nTe enviaremos un enlace de recuperación.", "OK");
    }
}
