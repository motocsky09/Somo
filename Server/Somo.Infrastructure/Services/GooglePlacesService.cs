using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Somo.Application.Interfaces;

namespace Somo.Infrastructure.Services;

public class GooglePlacesService : IGooglePlacesService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GooglePlacesService(IConfiguration configuration, HttpClient httpClient)
    {
        _apiKey = configuration["GooglePlaces:ApiKey"] ?? string.Empty;
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<GooglePlaceResult>> SearchVeterinaryClinicsAsync(
        double lat, double lng, double radiusMeters)
    {
var url = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json" +
          $"?location={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
          $"&radius={radiusMeters.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
          $"&type=veterinary_care" +
          $"&key={_apiKey}";

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        var json = JsonDocument.Parse(content);
        var results = new List<GooglePlaceResult>();

        if (!json.RootElement.TryGetProperty("results", out var placesArray))
            return results;

        foreach (var place in placesArray.EnumerateArray())
        {
            var location = place.GetProperty("geometry").GetProperty("location");
            results.Add(new GooglePlaceResult
            {
                PlaceId = place.GetProperty("place_id").GetString() ?? string.Empty,
                Name = place.GetProperty("name").GetString() ?? string.Empty,
                Address = place.TryGetProperty("vicinity", out var addr)
                    ? addr.GetString() ?? string.Empty : string.Empty,
                Latitude = location.GetProperty("lat").GetDouble(),
                Longitude = location.GetProperty("lng").GetDouble()
            });
        }

        return results;
    }

    public async Task<(double Lat, double Lng)?> GeocodeAddressAsync(string address)
{
    var encodedAddress = Uri.EscapeDataString(address);
    var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={_apiKey}";

    var response = await _httpClient.GetAsync(url);
    var content = await response.Content.ReadAsStringAsync();
    
    // TEMPORAR - pentru debug, șterge după ce merge
    Console.WriteLine($"=== GEOCODE DEBUG ===");
    Console.WriteLine($"Address: {address}");
    Console.WriteLine($"API Key: {_apiKey[..10]}...");
    Console.WriteLine($"Response: {content}");
    Console.WriteLine($"====================");

    var json = JsonDocument.Parse(content);

    if (!json.RootElement.TryGetProperty("results", out var results))
        return null;

    var firstResult = results.EnumerateArray().FirstOrDefault();
    if (firstResult.ValueKind == JsonValueKind.Undefined)
        return null;

    var location = firstResult
        .GetProperty("geometry")
        .GetProperty("location");

    return (
        location.GetProperty("lat").GetDouble(),
        location.GetProperty("lng").GetDouble()
    );
}
}