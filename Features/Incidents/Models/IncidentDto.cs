using System;

namespace TravelSecure.Mobile.Features.Incidents.Models;

public class IncidentDto
{
    public int Id { get; set; }
    public int Type { get; set; }
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}
