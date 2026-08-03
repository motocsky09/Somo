namespace Somo.Application.Interfaces;

public interface IPlacesCacheService
{
    TimeSpan Lifetime { get; }
    Task<CachedCitySearch?> GetAsync(string city, double radiusKm);
    Task SaveAsync(CachedCitySearch entry);
}

public class CachedCitySearch
{
    public string City { get; set; } = string.Empty;
    public double RadiusKm { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CachedAtUtc { get; set; }
    public List<GooglePlaceResult> Places { get; set; } = new();
}
