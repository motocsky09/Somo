using Somo.Domain.Entities;

namespace Somo.Domain.Interfaces;

public interface IMedicalRecordRepository
{
    Task<IEnumerable<MedicalRecord>> GetAllByPetIdAsync(string petId);
    Task<IEnumerable<MedicalRecord>> GetByAppointmentIdAsync(string appointmentId);
    Task<MedicalRecord?> GetByIdAsync(string id);
    Task CreateAsync(MedicalRecord record);
    Task UpdateAsync(MedicalRecord record);
    Task DeleteAsync(string id);
}
