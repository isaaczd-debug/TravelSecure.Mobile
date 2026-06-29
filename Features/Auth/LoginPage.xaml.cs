using TravelSecure.Mobile.Features.Auth.ViewModels;

namespace TravelSecure.Mobile.Features.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}