using Somo.Domain.Entities;

namespace Somo.Domain.Interfaces;

public interface IVeterinaryClinicRepository
{
    Task<IEnumerable<VeterinaryClinic>> GetAllAsync();
    Task<IEnumerable<VeterinaryClinic>> GetApprovedAsync();
    Task<IEnumerable<VeterinaryClinic>> GetByStatusAsync(ClinicStatus status);
    Task<IEnumerable<VeterinaryClinic>> GetByAdminIdAsync(string adminId);
    Task<VeterinaryClinic?> GetByIdAsync(string id);
    Task<IEnumerable<VeterinaryClinic>> GetByCityAsync(string city);
    Task<IEnumerable<VeterinaryClinic>> GetNearbyAsync(double lat, double lng, double radiusKm);
    Task<long> ApproveLegacyClinicsAsync();
    Task CreateAsync(VeterinaryClinic clinic);
    Task UpdateAsync(VeterinaryClinic clinic);
    Task DeleteAsync(string id);
}