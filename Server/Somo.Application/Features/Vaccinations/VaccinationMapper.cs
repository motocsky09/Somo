using Somo.Application.DTOs;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Application.Features.Vaccinations;

/// <summary>
/// Îmbogățește vaccinurile cu numele medicului și al cabinetului, ca interfața să nu
/// mai facă un apel separat pentru fiecare.
/// </summary>
public class VaccinationMapper
{
    private readonly IVetRepository _vets;
    private readonly IVeterinaryClinicRepository _clinics;

    public VaccinationMapper(IVetRepository vets, IVeterinaryClinicRepository clinics)
    {
        _vets = vets;
        _clinics = clinics;
    }

    public async Task<List<VaccinationDto>> ToDtosAsync(IEnumerable<Vaccination> vaccinations)
    {
        var list = vaccinations.ToList();
        var vetNames = await ResolveVetNamesAsync(list.Select(v => v.VetId));
        var clinicNames = await ResolveClinicNamesAsync(list.Select(v => v.ClinicId));
        var today = DateTime.UtcNow.Date;

        return list
            .OrderByDescending(v => v.AdministeredOn)
            .Select(v => new VaccinationDto
            {
                Id = v.Id,
                PetId = v.PetId,
                VetId = v.VetId,
                ClinicId = v.ClinicId,
                VaccineCode = v.VaccineCode,
                VaccineName = v.VaccineName,
                BatchNumber = v.BatchNumber,
                Notes = v.Notes,
                AdministeredOn = v.AdministeredOn,
                NextDueOn = v.NextDueOn,
                ReminderSent = v.ReminderSentAtUtc is not null,
                VetName = vetNames.GetValueOrDefault(v.VetId, string.Empty),
                ClinicName = clinicNames.GetValueOrDefault(v.ClinicId, string.Empty),
                DaysUntilDue = (v.NextDueOn.Date - today).Days
            })
            .ToList();
    }

    private async Task<Dictionary<string, string>> ResolveVetNamesAsync(IEnumerable<string> vetIds)
    {
        var names = new Dictionary<string, string>();
        foreach (var id in vetIds.Distinct().Where(id => !string.IsNullOrEmpty(id)))
        {
            var vet = await _vets.GetByIdAsync(id);
            if (vet is not null)
                names[id] = $"Dr. {vet.FirstName} {vet.LastName}".Trim();
        }
        return names;
    }

    private async Task<Dictionary<string, string>> ResolveClinicNamesAsync(IEnumerable<string> clinicIds)
    {
        var names = new Dictionary<string, string>();
        foreach (var id in clinicIds.Distinct().Where(id => !string.IsNullOrEmpty(id)))
        {
            var clinic = await _clinics.GetByIdAsync(id);
            if (clinic is not null)
                names[id] = clinic.Name;
        }
        return names;
    }
}
