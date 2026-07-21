using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TravelSecure.Mobile.Features.Auth.Services;
using TravelSecure.Mobile.Features.Incidents.Models;

namespace TravelSecure.Mobile.Features.Incidents.Services;

public class IncidentApiService
{
    // Evento que notifica cambios (nuevo incidente creado)
    public event EventHandler? DataChanged;

    private readonly HttpClient _httpClient;
    private readonly TokenStorage _tokenStorage;
    private readonly JsonSerializerOptions _jsonOptions;

    public IncidentApiService(TokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7224/") // <-- Ajusta la URL si es necesario
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task SetAuthTokenAsync()
    {
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        else
            _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<HttpResponseMessage> CreateIncidentAsync(CreateIncidentRequest request)
    {
        await SetAuthTokenAsync();
        var response = await _httpClient.PostAsJsonAsync("api/incidents", request, _jsonOptions);
        try
        {
            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine("IncidentApiService: CreateIncidentAsync success, raising DataChanged");
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateIncidentAsync DataChanged invoke error: {ex}");
        }

        return response;
    }

    public async Task<IEnumerable<IncidentDto>> GetRecentIncidentsAsync()
    {
        await SetAuthTokenAsync();
        // El backend expone la consulta de usuario en el controlador IncidentsQueryController
        // (ruta: api/IncidentsQuery/me). Usamos ese endpoint para obtener los incidentes del usuario.
        try
        {
            // Log token presence (no imprimir token completo por seguridad)
            try
            {
                var token = await _tokenStorage.GetAccessTokenAsync();
                System.Diagnostics.Debug.WriteLine($"IncidentApiService: token present={!string.IsNullOrEmpty(token)}, length={token?.Length ?? 0}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IncidentApiService: error reading token: {ex.Message}");
            }

            var response = await _httpClient.GetAsync("api/IncidentsQuery/me");
            System.Diagnostics.Debug.WriteLine($"IncidentApiService: GET api/IncidentsQuery/me -> {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"IncidentApiService: response content: {json}");

            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<IncidentDto>();
            }

            IEnumerable<IncidentDto> result = JsonSerializer.Deserialize<IEnumerable<IncidentDto>>(json, _jsonOptions) ?? Array.Empty<IncidentDto>();
            System.Diagnostics.Debug.WriteLine($"IncidentApiService: parsed incidents count={result.Count()} ");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetRecentIncidentsAsync exception: {ex}");
            return Array.Empty<IncidentDto>();
        }
    }

    // Obtener todos los incidentes (para módulo de Alertas)
    public async Task<IEnumerable<IncidentDto>> GetAllIncidentsAsync()
    {
        await SetAuthTokenAsync();
        try
        {
            // Algunos backends pueden no exponer /all; intentamos /all y si falla usamos /me
            var response = await _httpClient.GetAsync("api/IncidentsQuery/all");
            System.Diagnostics.Debug.WriteLine($"IncidentApiService: GET api/IncidentsQuery/all -> {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine("IncidentApiService: /all no disponible, intentando /me");
                response = await _httpClient.GetAsync("api/IncidentsQuery/me");
                System.Diagnostics.Debug.WriteLine($"IncidentApiService: GET api/IncidentsQuery/me -> {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"IncidentApiService: response all/me content: {json}");
            if (!response.IsSuccessStatusCode) return Array.Empty<IncidentDto>();
            return JsonSerializer.Deserialize<IEnumerable<IncidentDto>>(json, _jsonOptions) ?? Array.Empty<IncidentDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAllIncidentsAsync exception: {ex}");
            return Array.Empty<IncidentDto>();
        }
    }


    // SSE (Server Sent Events) simple: conectar a un endpoint y procesar líneas "data: {...}"
    public async Task StartSseAsync(Func<IncidentDto, Task> onIncident, CancellationToken ct)
    {
        await SetAuthTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "api/IncidentsQuery/stream");
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new System.IO.StreamReader(stream);
        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            // SSE: líneas que empiezan por "data: "
            if (line.StartsWith("data: "))
            {
                var json = line.Substring(6);
                try
                {
                    var incident = JsonSerializer.Deserialize<IncidentDto>(json, _jsonOptions);
                    if (incident != null) await onIncident(incident);
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"SSE parse error: {ex}"); }
            }
        }

    }

    // Permite a consumidores externos solicitar que se dispare el evento DataChanged
    // (invocar eventos directamente desde fuera de la clase no está permitido en C#)
    public void NotifyDataChanged()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("IncidentApiService: NotifyDataChanged called");
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"NotifyDataChanged error: {ex}");
        }
    }

}
