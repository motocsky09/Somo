using Somo.Domain.Entities;

namespace Somo.Domain.Interfaces;

public interface IVeterinaryClinicRepository
{
    Task<IEnumerable<VeterinaryClinic>> GetAllAsync();
    Task<VeterinaryClinic?> GetByIdAsync(string id);
    Task<IEnumerable<VeterinaryClinic>> GetByCityAsync(string city);
    Task<IEnumerable<VeterinaryClinic>> GetNearbyAsync(double lat, double lng, double radiusKm);  // ← nou
    Task CreateAsync(VeterinaryClinic clinic);
    Task UpdateAsync(VeterinaryClinic clinic);
    Task DeleteAsync(string id);
}