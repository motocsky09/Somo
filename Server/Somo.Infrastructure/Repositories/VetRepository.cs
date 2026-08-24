using MongoDB.Driver;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Infrastructure.Repositories;

public class VetRepository : IVetRepository
{
    private readonly IMongoCollection<Vet> _collection;

    public VetRepository(IMongoDatabase database)
        => _collection = database.GetCollection<Vet>("Vets");

    public async Task<IEnumerable<Vet>> GetAllAsync()
        => await _collection.Find(_ => true).ToListAsync();

    public async Task<IEnumerable<Vet>> GetByClinicIdAsync(string clinicId)
        => await _collection.Find(v => v.ClinicIds.Contains(clinicId)).ToListAsync();

    public async Task<Vet?> GetByIdAsync(string id)
        => await _collection.Find(v => v.Id == id).FirstOrDefaultAsync();

    public async Task<Vet?> GetByUserIdAsync(string userId)
        => await _collection.Find(v => v.UserId == userId).FirstOrDefaultAsync();

    public async Task CreateAsync(Vet vet)
        => await _collection.InsertOneAsync(vet);

    public async Task UpdateAsync(Vet vet)
        => await _collection.ReplaceOneAsync(v => v.Id == vet.Id, vet);

    public async Task DeleteAsync(string id)
        => await _collection.DeleteOneAsync(v => v.Id == id);
}