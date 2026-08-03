using System.Globalization;
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
        var url = "https://maps.googleapis.com/maps/api/place/nearbysearch/json" +
                  $"?location={lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}" +
                  $"&radius={radiusMeters.ToString(CultureInfo.InvariantCulture)}" +
                  "&type=veterinary_care" +
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

    public async Task<(double Lat, double Lng)?> GeocodeCityAsync(string city)
    {
        var url = "https://maps.googleapis.com/maps/api/geocode/json" +
                  $"?address={Uri.EscapeDataString(city)}" +
                  "&components=country:RO" +
                  $"&key={_apiKey}";

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        var json = JsonDocument.Parse(content);

        if (!json.RootElement.TryGetProperty("results", out var results))
            return null;

        foreach (var result in results.EnumerateArray())
        {
            if (!IsLocality(result))
                continue;

            var location = result.GetProperty("geometry").GetProperty("location");
            return (
                location.GetProperty("lat").GetDouble(),
                location.GetProperty("lng").GetDouble()
            );
        }

        return null;
    }

    private static bool IsLocality(JsonElement result)
    {
        if (!result.TryGetProperty("types", out var types))
            return false;

        return types.EnumerateArray()
            .Select(t => t.GetString())
            .Any(t => t is "locality" or "postal_town" or "administrative_area_level_2" or "administrative_area_level_3");
    }

    public async Task<(double Lat, double Lng)?> GeocodeAddressAsync(string address)
    {
        var encodedAddress = Uri.EscapeDataString(address);
        var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={_apiKey}";

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

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
