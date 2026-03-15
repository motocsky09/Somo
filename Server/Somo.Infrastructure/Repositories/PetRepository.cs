using MongoDB.Driver;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Infrastructure.Repositories;

public class PetRepository : IPetRepository
{
    private readonly IMongoCollection<Pet> _collection;

    public PetRepository(IMongoDatabase database)
        => _collection = database.GetCollection<Pet>("Pets");

    public async Task<IEnumerable<Pet>> GetAllByOwnerIdAsync(string ownerId)
        => await _collection.Find(p => p.OwnerId == ownerId).ToListAsync();

    public async Task<Pet?> GetByIdAsync(string id)
        => await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Pet pet)
        => await _collection.InsertOneAsync(pet);

    public async Task UpdateAsync(Pet pet)
        => await _collection.ReplaceOneAsync(p => p.Id == pet.Id, pet);

    public async Task DeleteAsync(string id)
        => await _collection.DeleteOneAsync(p => p.Id == id);
}