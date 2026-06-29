namespace TravelSecure.Mobile.Helpers;

public static class Constants
{
    public static readonly Dictionary<string, int> LicenseMap = new()
    {
        { "A-I  — Moto / Mototaxi", 0 },
        { "B-IIb — Auto / Camioneta", 1 },
        { "A-IIb — Camión Mediano", 2 },
        { "A-IIIb — Semi-articulado", 3 },
        { "A-IIIc — Combinado / Convoy", 4 }
    };
}