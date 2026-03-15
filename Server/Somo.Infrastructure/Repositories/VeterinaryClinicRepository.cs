using MongoDB.Driver;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Infrastructure.Repositories;

public class VeterinaryClinicRepository : IVeterinaryClinicRepository
{
    private readonly IMongoCollection<VeterinaryClinic> _collection;

    public VeterinaryClinicRepository(IMongoDatabase database)
        => _collection = database.GetCollection<VeterinaryClinic>("VeterinaryClinics");

    public async Task<IEnumerable<VeterinaryClinic>> GetAllAsync()
        => await _collection.Find(_ => true).ToListAsync();

    public async Task<VeterinaryClinic?> GetByIdAsync(string id)
        => await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<VeterinaryClinic>> GetByCityAsync(string city)
        => await _collection.Find(c => c.City == city).ToListAsync();

    public async Task<IEnumerable<VeterinaryClinic>> GetNearbyAsync(double lat, double lng, double radiusKm)
    {
        var all = await _collection.Find(_ => true).ToListAsync();
        return all.Where(c => CalculateDistance(lat, lng, c.Latitude, c.Longitude) <= radiusKm);
    }

    public async Task CreateAsync(VeterinaryClinic clinic)
        => await _collection.InsertOneAsync(clinic);

    public async Task UpdateAsync(VeterinaryClinic clinic)
        => await _collection.ReplaceOneAsync(c => c.Id == clinic.Id, clinic);

    public async Task DeleteAsync(string id)
        => await _collection.DeleteOneAsync(c => c.Id == id);

    // Formula Haversine
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // raza Pământului în km
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}