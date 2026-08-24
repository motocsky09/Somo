using MongoDB.Driver;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Infrastructure.Repositories;

public class VaccinationRepository : IVaccinationRepository
{
    private readonly IMongoCollection<Vaccination> _collection;

    public VaccinationRepository(IMongoDatabase database)
        => _collection = database.GetCollection<Vaccination>("Vaccinations");

    public async Task<IEnumerable<Vaccination>> GetAllByPetIdAsync(string petId)
        => await _collection.Find(v => v.PetId == petId)
            .SortByDescending(v => v.AdministeredOn)
            .ToListAsync();

    public async Task<IEnumerable<Vaccination>> GetByClinicIdAsync(string clinicId)
        => await _collection.Find(v => v.ClinicId == clinicId)
            .SortByDescending(v => v.AdministeredOn)
            .ToListAsync();

    public async Task<Vaccination?> GetByIdAsync(string id)
        => await _collection.Find(v => v.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<Vaccination>> GetDueWithoutReminderAsync(
        DateTime fromInclusive, DateTime toInclusive)
        => await _collection.Find(v =>
                v.ReminderSentAtUtc == null &&
                v.NextDueOn >= fromInclusive &&
                v.NextDueOn <= toInclusive)
            .ToListAsync();

    public async Task CreateAsync(Vaccination vaccination)
        => await _collection.InsertOneAsync(vaccination);

    public async Task UpdateAsync(Vaccination vaccination)
        => await _collection.ReplaceOneAsync(v => v.Id == vaccination.Id, vaccination);

    public async Task DeleteAsync(string id)
        => await _collection.DeleteOneAsync(v => v.Id == id);
}
