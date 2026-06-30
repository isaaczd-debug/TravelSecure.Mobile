using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TravelSecure.Mobile.Features.Auth.Models;

namespace TravelSecure.Mobile.Features.Auth.Service;

public class WeatherService
{
    private readonly HttpClient _httpClient;

    public WeatherService()
    {
        // Agregamos un handler para ignorar problemas de certificados SSL locales si los hubiera
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<WeatherResponse> GetWeatherAsync(string city, string token)
    {
        try
        {
            // Si usas emulador Android de Google, recuerda usar "
            string url = $"http://10.0.2.2:5138/api/WeatherForecast/ruta/{city}";

            _httpClient.DefaultRequestHeaders.Authorization = null; // Limpiamos cabeceras previas

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Definimos las opciones de deserialización para CamelCase
            var opciones = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // ⚠️ PASAMOS LAS OPCIONES COMO SEGUNDO PARÁMETRO
            var response = await _httpClient.GetFromJsonAsync<WeatherResponse>(url, opciones);
            return response;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ERROR CRÍTICO EN WEATHER_SERVICE: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
            }
            return null;
        }
    }
}