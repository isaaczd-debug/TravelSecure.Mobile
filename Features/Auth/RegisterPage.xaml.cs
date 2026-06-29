using TravelSecure.Mobile.Features.Auth.ViewModels;

namespace TravelSecure.Mobile.Features.Auth;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}