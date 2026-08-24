using Somo.Application.DTOs;
using Somo.Application.Interfaces;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Application.Features.Appointments.Queries;

/// <summary>
/// Compune programările cu animalul, medicul și proprietarul, ca fiecare listă
/// afișată să vină dintr-un singur apel.
/// </summary>
public class AppointmentDetailsMapper
{
    private readonly IPetRepository _pets;
    private readonly IVetRepository _vets;
    private readonly IUserDirectory _users;

    public AppointmentDetailsMapper(
        IPetRepository pets,
        IVetRepository vets,
        IUserDirectory users)
    {
        _pets = pets;
        _vets = vets;
        _users = users;
    }

    public async Task<List<AppointmentDetailsDto>> ToDtosAsync(IEnumerable<Appointment> appointments)
    {
        var list = appointments.ToList();

        var pets = new Dictionary<string, Pet?>();
        var vets = new Dictionary<string, Vet?>();
        var owners = new Dictionary<string, UserContact?>();

        foreach (var petId in Ids(list.Select(a => a.PetId)))
            pets[petId] = await _pets.GetByIdAsync(petId);

        foreach (var vetId in Ids(list.Select(a => a.VetId)))
            vets[vetId] = await _vets.GetByIdAsync(vetId);

        foreach (var ownerId in Ids(list.Select(a => a.OwnerId)))
            owners[ownerId] = await _users.GetContactAsync(ownerId);

        return list.Select(a =>
        {
            var pet = pets.GetValueOrDefault(a.PetId);
            var vet = vets.GetValueOrDefault(a.VetId);
            var owner = owners.GetValueOrDefault(a.OwnerId);

            return new AppointmentDetailsDto
            {
                Id = a.Id,
                PetId = a.PetId,
                VetId = a.VetId,
                ClinicId = a.ClinicId,
                OwnerId = a.OwnerId,
                DateTime = a.DateTime,
                Reason = a.Reason,
                Status = (int)a.Status,
                Pet = pet is null ? null : new AppointmentPetDto
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    Species = pet.Species,
                    Breed = pet.Breed,
                    Age = pet.Age,
                    Weight = pet.Weight,
                    PhotoUrl = pet.PhotoUrl
                },
                Vet = vet is null ? null : new AppointmentVetDto
                {
                    Id = vet.Id,
                    FirstName = vet.FirstName,
                    LastName = vet.LastName,
                    Specialization = vet.Specialization
                },
                Owner = owner is null ? null : new AppointmentOwnerDto
                {
                    Id = owner.Id,
                    Username = owner.Username,
                    FirstName = owner.FirstName,
                    LastName = owner.LastName,
                    Email = owner.Email,
                    Phone = owner.Phone,
                    ProfilePhotoUrl = owner.ProfilePhotoUrl
                }
            };
        }).ToList();
    }

    private static IEnumerable<string> Ids(IEnumerable<string> values)
        => values.Distinct().Where(id => !string.IsNullOrEmpty(id));
}
