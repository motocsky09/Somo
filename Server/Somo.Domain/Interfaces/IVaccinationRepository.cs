using Somo.Domain.Entities;

namespace Somo.Domain.Interfaces;

public interface IVaccinationRepository
{
    Task<IEnumerable<Vaccination>> GetAllByPetIdAsync(string petId);
    Task<IEnumerable<Vaccination>> GetByClinicIdAsync(string clinicId);
    Task<Vaccination?> GetByIdAsync(string id);

    /// <summary>
    /// Vaccinurile cu rapelul scadent în intervalul dat pentru care nu a plecat
    /// încă niciun reminder.
    /// </summary>
    Task<IEnumerable<Vaccination>> GetDueWithoutReminderAsync(DateTime fromInclusive, DateTime toInclusive);

    Task CreateAsync(Vaccination vaccination);
    Task UpdateAsync(Vaccination vaccination);
    Task DeleteAsync(string id);
}
