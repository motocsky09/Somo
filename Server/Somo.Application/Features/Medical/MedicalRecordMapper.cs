using Somo.Application.DTOs;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Application.Features.Medical;

public class MedicalRecordMapper
{
    private readonly IVetRepository _vets;
    private readonly IVeterinaryClinicRepository _clinics;

    public MedicalRecordMapper(IVetRepository vets, IVeterinaryClinicRepository clinics)
    {
        _vets = vets;
        _clinics = clinics;
    }

    public async Task<List<MedicalRecordDto>> ToDtosAsync(IEnumerable<MedicalRecord> records)
    {
        var list = records.ToList();
        var vetNames = new Dictionary<string, string>();
        var clinicNames = new Dictionary<string, string>();

        foreach (var id in list.Select(r => r.VetId).Distinct().Where(id => !string.IsNullOrEmpty(id)))
        {
            var vet = await _vets.GetByIdAsync(id);
            if (vet is not null)
                vetNames[id] = $"Dr. {vet.FirstName} {vet.LastName}".Trim();
        }

        foreach (var id in list.Select(r => r.ClinicId).Distinct().Where(id => !string.IsNullOrEmpty(id)))
        {
            var clinic = await _clinics.GetByIdAsync(id);
            if (clinic is not null)
                clinicNames[id] = clinic.Name;
        }

        return list
            .OrderByDescending(r => r.Date)
            .Select(r => new MedicalRecordDto
            {
                Id = r.Id,
                PetId = r.PetId,
                VetId = r.VetId,
                ClinicId = r.ClinicId,
                AppointmentId = r.AppointmentId,
                Date = r.Date,
                Diagnosis = r.Diagnosis,
                Treatment = r.Treatment,
                Notes = r.Notes,
                Weight = r.Weight,
                Temperature = r.Temperature,
                VetName = vetNames.GetValueOrDefault(r.VetId, string.Empty),
                ClinicName = clinicNames.GetValueOrDefault(r.ClinicId, string.Empty)
            })
            .ToList();
    }
}
