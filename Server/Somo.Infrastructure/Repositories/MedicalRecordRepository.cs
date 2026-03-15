using MongoDB.Driver;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Infrastructure.Repositories;

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly IMongoCollection<MedicalRecord> _collection;

    public MedicalRecordRepository(IMongoDatabase database)
        => _collection = database.GetCollection<MedicalRecord>("MedicalRecords");

    public async Task<IEnumerable<MedicalRecord>> GetAllByPetIdAsync(string petId)
        => await _collection.Find(r => r.PetId == petId).ToListAsync();

    public async Task<MedicalRecord?> GetByIdAsync(string id)
        => await _collection.Find(r => r.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(MedicalRecord record)
        => await _collection.InsertOneAsync(record);

    public async Task UpdateAsync(MedicalRecord record)
        => await _collection.ReplaceOneAsync(r => r.Id == record.Id, record);

    public async Task DeleteAsync(string id)
        => await _collection.DeleteOneAsync(r => r.Id == id);
}