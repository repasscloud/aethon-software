namespace Aethon.Shared.Locations;

public sealed class LocationDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = "";
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
