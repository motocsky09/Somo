namespace Somo.Application.Interfaces;

public interface IGooglePlacesService
{
    Task<IEnumerable<GooglePlaceResult>> SearchVeterinaryClinicsAsync(double lat, double lng, double radiusMeters);
    Task<(double Lat, double Lng)?> GeocodeAddressAsync(string address);
}

public class GooglePlaceResult
{
    public string PlaceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsInDatabase { get; set; } = false;
}