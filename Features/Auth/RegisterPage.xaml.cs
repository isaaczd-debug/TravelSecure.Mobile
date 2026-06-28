namespace TravelSecure.Mobile.Features.Auth;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnCrearCuenta(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryNombre.Text) ||

            string.IsNullOrWhiteSpace(EntryEmail.Text) ||

            string.IsNullOrWhiteSpace(EntryDni.Text) ||

            string.IsNullOrWhiteSpace(EntryPassword.Text) ||

            PickerLicencia.SelectedIndex == -1)
        {
            await DisplayAlert("Campos incompletos",
                "Por favor completa todos los campos obligatorios.", "OK");
            return;
        }

        if (EntryPassword.Text.Length < 8)
        {
            await DisplayAlert("Contraseña débil",
                "La contraseña debe tener al menos 8 caracteres.", "OK");

            return;
        }

        BtnCrear.IsEnabled = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        await Task.Delay(1800);

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        BtnCrear.IsEnabled = true;

        await DisplayAlert("✅ ¡Registro exitoso!",
            $"Bienvenido {EntryNombre.Text.Split(' ')[0]}.\nYa puedes iniciar sesión con tu cuenta.",
            "Continuar");

        await Shell.Current.GoToAsync("..");
    }

    private async void OnGoBack(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
