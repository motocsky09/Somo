using Somo.Domain.Entities;

namespace Somo.Domain.Interfaces;

public interface IVetRepository
{
    Task<IEnumerable<Vet>> GetAllAsync();
    Task<Vet?> GetByIdAsync(string id);
    Task CreateAsync(Vet vet);
    Task UpdateAsync(Vet vet);
    Task DeleteAsync(string id);
}