using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Somo.API.Entities;
using Somo.Application.Common;
using Somo.Application.DTOs;
using Somo.Application.Features.Appointments.Commands;
using Somo.Application.Features.Appointments.Queries;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _repo;
    private readonly IVeterinaryClinicRepository _clinicRepo;
    private readonly IPetRepository _petRepo;
    private readonly IVetRepository _vetRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CreateAppointmentCommand _createCommand;
    private readonly GetAvailableSlotsQuery _slotsQuery;

    public AppointmentsController(
        IAppointmentRepository repo,
        IVeterinaryClinicRepository clinicRepo,
        IPetRepository petRepo,
        IVetRepository vetRepo,
        UserManager<ApplicationUser> userManager,
        CreateAppointmentCommand createCommand,
        GetAvailableSlotsQuery slotsQuery)
    {
        _repo = repo;
        _clinicRepo = clinicRepo;
        _petRepo = petRepo;
        _vetRepo = vetRepo;
        _userManager = userManager;
        _createCommand = createCommand;
        _slotsQuery = slotsQuery;
    }

    private string? CurrentUserId => User.FindFirst("id")?.Value;

    [HttpGet("owner/{ownerId}")]
    public async Task<IActionResult> GetByOwner(string ownerId)
        => Ok(await _repo.GetAllByOwnerIdAsync(ownerId));

    [HttpGet("vet/{vetId}")]
    public async Task<IActionResult> GetByVet(string vetId)
        => Ok(await _repo.GetAllByVetIdAsync(vetId));

    [HttpGet("by-clinic/{clinicId}")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> GetByClinic(string clinicId)
    {
        var clinic = await _clinicRepo.GetByIdAsync(clinicId);
        if (clinic is null) return NotFound();
        if (clinic.AdminId != CurrentUserId) return Forbid();

        var appointments = await _repo.GetByClinicIdAsync(clinicId);
        return Ok(await BuildDetailsAsync(appointments));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var appointment = await _repo.GetByIdAsync(id);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpGet("{id}/details")]
    [Authorize]
    public async Task<IActionResult> GetDetails(string id)
    {
        var appointment = await _repo.GetByIdAsync(id);
        if (appointment is null) return NotFound();

        if (appointment.OwnerId != CurrentUserId)
        {
            var clinic = await _clinicRepo.GetByIdAsync(appointment.ClinicId);
            if (clinic is null || clinic.AdminId != CurrentUserId) return Forbid();
        }

        var details = await BuildDetailsAsync(new[] { appointment });
        return Ok(details.Single());
    }

    [HttpGet("available-slots")]
    public async Task<IActionResult> GetAvailableSlots(string vetId, DateTime date)
        => Ok(await _slotsQuery.ExecuteAsync(vetId, date));

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateAppointmentDto dto)
    {
        var ownerId = CurrentUserId;
        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized(new { error = "Token invalid." });

        var (success, error) = await _createCommand.ExecuteAsync(dto, ownerId);
        if (!success) return BadRequest(new { error });
        return Ok(new { message = "Programare creată cu succes." });
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] int status)
    {
        var appointment = await _repo.GetByIdAsync(id);
        if (appointment is null) return NotFound();

        appointment.Status = (AppointmentStatus)status;
        await _repo.UpdateAsync(appointment);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> Update(string id, Appointment appointment)
    {
        appointment.Id = id;
        await _repo.UpdateAsync(appointment);
        return NoContent();
    }

    private async Task<List<AppointmentDetailsDto>> BuildDetailsAsync(IEnumerable<Appointment> appointments)
    {
        var list = appointments.ToList();

        var pets = new Dictionary<string, Pet?>();
        var vets = new Dictionary<string, Vet?>();
        var owners = new Dictionary<string, ApplicationUser?>();

        foreach (var petId in list.Select(a => a.PetId).Distinct().Where(id => !string.IsNullOrEmpty(id)))
            pets[petId] = await _petRepo.GetByIdAsync(petId);

        foreach (var vetId in list.Select(a => a.VetId).Distinct().Where(id => !string.IsNullOrEmpty(id)))
            vets[vetId] = await _vetRepo.GetByIdAsync(vetId);

        foreach (var ownerId in list.Select(a => a.OwnerId).Distinct().Where(id => !string.IsNullOrEmpty(id)))
            owners[ownerId] = await _userManager.FindByIdAsync(ownerId);

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
                    Id = owner.Id.ToString(),
                    Username = owner.UserName ?? string.Empty,
                    FirstName = owner.FirstName ?? string.Empty,
                    LastName = owner.LastName ?? string.Empty,
                    Email = owner.Email ?? string.Empty,
                    Phone = owner.PhoneNumber ?? string.Empty,
                    ProfilePhotoUrl = owner.ProfilePhotoUrl
                }
            };
        }).ToList();
    }
}
