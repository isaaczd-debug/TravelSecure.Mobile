using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace TravelSecure.Mobile.Features.Auth.Models;

public class WeatherResponse
{
    [JsonPropertyName("name")]
    public string name { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string country { get; set; } = string.Empty;

    [JsonPropertyName("temp")]
    public double temp { get; set; }

    [JsonPropertyName("feelsLike")]
    public double feelsLike { get; set; }

    [JsonPropertyName("humidity")]
    public int humidity { get; set; }

    [JsonPropertyName("description")]
    public string description { get; set; } = string.Empty;

    [JsonPropertyName("windSpeed")]
    public double windSpeed { get; set; }

}
