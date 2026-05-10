using MongoDB.Driver;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly IMongoCollection<Appointment> _collection;

    public AppointmentRepository(IMongoDatabase database)
        => _collection = database.GetCollection<Appointment>("Appointments");

    public async Task<IEnumerable<Appointment>> GetAllByOwnerIdAsync(string ownerId)
        => await _collection.Find(a => a.OwnerId == ownerId).ToListAsync();

    public async Task<IEnumerable<Appointment>> GetAllByVetIdAsync(string vetId)
        => await _collection.Find(a => a.VetId == vetId).ToListAsync();

    public async Task<Appointment?> GetByIdAsync(string id)
        => await _collection.Find(a => a.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<Appointment>> GetByClinicIdAsync(string clinicId)
        => await _collection.Find(a => a.ClinicId == clinicId).ToListAsync();

    public async Task CreateAsync(Appointment appointment)
        => await _collection.InsertOneAsync(appointment);

    public async Task UpdateAsync(Appointment appointment)
        => await _collection.ReplaceOneAsync(a => a.Id == appointment.Id, appointment);

    public async Task DeleteAsync(string id)
        => await _collection.DeleteOneAsync(a => a.Id == id);
}