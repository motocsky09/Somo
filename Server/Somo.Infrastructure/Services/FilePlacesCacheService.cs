using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Somo.Application.Interfaces;

namespace Somo.Infrastructure.Services;

public class FilePlacesCacheService : IPlacesCacheService
{
    private const string DefaultFile = "Cache/google-places.json";
    private const double DefaultLifetimeDays = 14;

    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _filePath;
    private Dictionary<string, CachedCitySearch>? _entries;

    public FilePlacesCacheService(IConfiguration configuration)
    {
        var configuredPath = configuration["GooglePlaces:CacheFile"] ?? DefaultFile;
        _filePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(Directory.GetCurrentDirectory(), configuredPath);

        var days = double.TryParse(
            configuration["GooglePlaces:CacheLifetimeDays"],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var configuredDays) && configuredDays > 0
            ? configuredDays
            : DefaultLifetimeDays;

        Lifetime = TimeSpan.FromDays(days);
    }

    public TimeSpan Lifetime { get; }

    public async Task<CachedCitySearch?> GetAsync(string city, double radiusKm)
    {
        await _gate.WaitAsync();
        try
        {
            var entries = await LoadAsync();
            if (!entries.TryGetValue(BuildKey(city, radiusKm), out var entry))
                return null;

            return IsExpired(entry) ? null : entry;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(CachedCitySearch entry)
    {
        await _gate.WaitAsync();
        try
        {
            var entries = await LoadAsync();
            entries[BuildKey(entry.City, entry.RadiusKm)] = entry;

            foreach (var key in entries.Where(e => IsExpired(e.Value)).Select(e => e.Key).ToList())
                entries.Remove(key);

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(entries, SerializerOptions));
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<Dictionary<string, CachedCitySearch>> LoadAsync()
    {
        if (_entries is not null)
            return _entries;

        if (!File.Exists(_filePath))
            return _entries = new Dictionary<string, CachedCitySearch>();

        try
        {
            var content = await File.ReadAllTextAsync(_filePath);
            _entries = JsonSerializer.Deserialize<Dictionary<string, CachedCitySearch>>(content)
                       ?? new Dictionary<string, CachedCitySearch>();
        }
        catch (JsonException)
        {
            _entries = new Dictionary<string, CachedCitySearch>();
        }

        return _entries;
    }

    private bool IsExpired(CachedCitySearch entry)
        => DateTime.UtcNow - entry.CachedAtUtc > Lifetime;

    private static string BuildKey(string city, double radiusKm)
        => $"{city.Trim().ToLowerInvariant()}|{radiusKm.ToString("0.##", CultureInfo.InvariantCulture)}";
}
