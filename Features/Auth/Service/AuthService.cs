using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TravelSecure.Mobile.Features.Auth.Models;

namespace TravelSecure.Mobile.Features.Auth.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly TokenStorage _tokenStorage;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthService(TokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7224/")  // ← TU URL
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // Método para poner el token en las peticiones
    public void SetAuthToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    // LOGIN
    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var request = new LoginRequest { Email = email, Password = password };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/auth/login", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseJson, _jsonOptions)!;

        // Guardar tokens
        await _tokenStorage.SaveTokensAsync(loginResponse.AccessToken, loginResponse.RefreshToken);
        SetAuthToken(loginResponse.AccessToken);

        return loginResponse;
    }

    // REGISTER
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/auth/register", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RegisterResponse>(responseJson, _jsonOptions)!;
    }

    // LOGOUT
    public void Logout()
    {
        _tokenStorage.ClearTokens();
        SetAuthToken(string.Empty);
    }

    // RESTAURAR SESIÓN
    public async Task RestoreSessionAsync()
    {
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            SetAuthToken(token);
        }
    }

    // VERIFICAR SI ESTÁ AUTENTICADO
    public async Task<bool> IsAuthenticatedAsync()
    {
        return await _tokenStorage.IsAuthenticatedAsync();
    }
}