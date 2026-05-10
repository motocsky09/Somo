using Somo.Domain.Entities;

namespace Somo.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetAllByOwnerIdAsync(string ownerId);
    Task<IEnumerable<Appointment>> GetAllByVetIdAsync(string vetId);
    Task<IEnumerable<Appointment>> GetByClinicIdAsync(string clinicId);
    Task<Appointment?> GetByIdAsync(string id);
    Task CreateAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(string id);
}