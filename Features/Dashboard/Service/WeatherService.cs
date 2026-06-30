// Features/Dashboard/Services/WeatherService.cs
using System.Net.Http.Headers;
using System.Text.Json;
using TravelSecure.Mobile.Features.Auth.Services;
using TravelSecure.Mobile.Features.Dashboard.Model;
namespace TravelSecure.Mobile.Features.Dashboard.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly TokenStorage _tokenStorage;
    private readonly JsonSerializerOptions _jsonOptions;

    public WeatherService(TokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;

        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        _httpClient = new HttpClient(handler)
        {
            // 🔥 USA LA MISMA URL QUE AUTH SERVICE
            BaseAddress = new Uri("https://localhost:7224/")  // ← CAMBIADO
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<WeatherResponse> GetWeatherAsync(string city)
    {
        await SetAuthHeaderAsync();

        // 🔥 URL CORRECTA (UNA sola 'c')
        var url = $"api/WeatherForescast/ruta/{city}";
        System.Diagnostics.Debug.WriteLine($"📡 Llamando a: {_httpClient.BaseAddress}{url}");

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error {response.StatusCode}: {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<WeatherResponse>(json, _jsonOptions)!;
    }
}