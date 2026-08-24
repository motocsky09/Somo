using Somo.Domain.Entities;

namespace Somo.Domain.Interfaces;

public interface IVetRepository
{
    Task<IEnumerable<Vet>> GetAllAsync();
    Task<IEnumerable<Vet>> GetByClinicIdAsync(string clinicId);
    Task<Vet?> GetByIdAsync(string id);
    Task<Vet?> GetByUserIdAsync(string userId);
    Task CreateAsync(Vet vet);
    Task UpdateAsync(Vet vet);
    Task DeleteAsync(string id);
}
