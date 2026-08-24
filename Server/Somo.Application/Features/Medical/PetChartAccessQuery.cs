using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Application.Features.Medical;

/// <summary>
/// Ce poate face un utilizator cu fișa medicală și carnetul de vaccinare ale unui animal.
/// </summary>
public class ChartAccess
{
    public bool CanRead { get; init; }
    public bool CanWrite { get; init; }
    public Pet? Pet { get; init; }
    public Vet? Vet { get; init; }

    /// <summary>
    /// Cabinetul pe care se contabilizează înregistrarea, când utilizatorul poate scrie.
    /// </summary>
    public string ClinicId { get; init; } = string.Empty;

    public static readonly ChartAccess Denied = new();
}

/// <summary>
/// Proprietarul își citește fișa animalului, medicul o completează pentru animalele
/// văzute în cabinetele lui, iar administratorul de cabinet o poate consulta.
/// </summary>
public class PetChartAccessQuery
{
    private readonly IPetRepository _pets;
    private readonly IVetRepository _vets;
    private readonly IAppointmentRepository _appointments;
    private readonly IVeterinaryClinicRepository _clinics;

    public PetChartAccessQuery(
        IPetRepository pets,
        IVetRepository vets,
        IAppointmentRepository appointments,
        IVeterinaryClinicRepository clinics)
    {
        _pets = pets;
        _vets = vets;
        _appointments = appointments;
        _clinics = clinics;
    }

    public async Task<ChartAccess> ExecuteAsync(string petId, string userId, IEnumerable<string> roles)
    {
        var pet = await _pets.GetByIdAsync(petId);
        if (pet is null || string.IsNullOrEmpty(userId))
            return ChartAccess.Denied;

        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (pet.OwnerId == userId)
            return new ChartAccess { CanRead = true, Pet = pet };

        if (roleSet.Contains(Common.AppRoles.Vet))
        {
            var vet = await _vets.GetByUserIdAsync(userId);
            if (vet is not null)
            {
                var clinicId = await FindSharedClinicAsync(petId, vet);
                if (clinicId is not null)
                {
                    return new ChartAccess
                    {
                        CanRead = true,
                        CanWrite = true,
                        Pet = pet,
                        Vet = vet,
                        ClinicId = clinicId
                    };
                }
            }
        }

        if (roleSet.Contains(Common.AppRoles.ClinicAdmin))
        {
            var owned = (await _clinics.GetByAdminIdAsync(userId)).Select(c => c.Id).ToHashSet();
            if (owned.Count > 0)
            {
                var appointments = await _appointments.GetAllByOwnerIdAsync(pet.OwnerId);
                if (appointments.Any(a => a.PetId == petId && owned.Contains(a.ClinicId)))
                    return new ChartAccess { CanRead = true, Pet = pet };
            }
        }

        if (roleSet.Contains(Common.AppRoles.SomoAdmin))
            return new ChartAccess { CanRead = true, Pet = pet };

        return ChartAccess.Denied;
    }

    /// <summary>
    /// Cabinetul prin care medicul a intrat în contact cu animalul: fie o programare
    /// pe numele lui, fie una la unul dintre cabinetele în care lucrează.
    /// </summary>
    private async Task<string?> FindSharedClinicAsync(string petId, Vet vet)
    {
        var appointments = (await _appointments.GetAllByVetIdAsync(vet.Id))
            .Where(a => a.PetId == petId)
            .ToList();

        if (appointments.Count > 0)
        {
            return appointments
                .OrderByDescending(a => a.DateTime)
                .Select(a => a.ClinicId)
                .FirstOrDefault(id => !string.IsNullOrEmpty(id))
                ?? vet.ClinicIds.FirstOrDefault();
        }

        foreach (var clinicId in vet.ClinicIds)
        {
            var clinicAppointments = await _appointments.GetByClinicIdAsync(clinicId);
            if (clinicAppointments.Any(a => a.PetId == petId))
                return clinicId;
        }

        return null;
    }
}
