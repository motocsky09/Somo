using Somo.Domain.Entities;

namespace Somo.Domain.Interfaces;

public interface IPetRepository
{
    Task<IEnumerable<Pet>> GetAllByOwnerIdAsync(string ownerId);
    Task<Pet?> GetByIdAsync(string id);
    Task CreateAsync(Pet pet);
    Task UpdateAsync(Pet pet);
    Task DeleteAsync(string id);
}