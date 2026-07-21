using System;

namespace TravelSecure.Mobile.Features.Incidents.Models;

public class CreateIncidentRequest
{
    // En el backend el UserId se toma del token, por lo que aquí no es necesario
    public int Type { get; set; }
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
