// Features/Dashboard/Models/WeatherResponse.cs
namespace TravelSecure.Mobile.Features.Dashboard.Model;

public class WeatherResponse
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Temp { get; set; }
    public double FeelsLike { get; set; }
    public int Humidity { get; set; }
    public string Description { get; set; } = string.Empty;
    public double WindSpeed { get; set; }
    public int Pressure { get; set; }
    public int Visibility { get; set; }
    public int Timezone { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}