using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Somo.API.Services;
using Somo.Application.Common;
using Somo.Application.DTOs;
using Somo.Application.Features.Appointments.Queries;
using Somo.Application.Features.Medical;
using Somo.Application.Interfaces;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VetsController : ApiControllerBase
{
    private readonly IVetRepository _repo;
    private readonly IVeterinaryClinicRepository _clinicRepo;
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly AppointmentDetailsMapper _appointmentMapper;
    private readonly VetAccountProvisioner _accounts;
    private readonly INotificationService _notifications;
    private readonly PetChartAccessQuery _chartAccess;
    private readonly IUserDirectory _users;

    public VetsController(
        IVetRepository repo,
        IVeterinaryClinicRepository clinicRepo,
        IAppointmentRepository appointmentRepo,
        AppointmentDetailsMapper appointmentMapper,
        VetAccountProvisioner accounts,
        INotificationService notifications,
        PetChartAccessQuery chartAccess,
        IUserDirectory users)
    {
        _repo = repo;
        _clinicRepo = clinicRepo;
        _appointmentRepo = appointmentRepo;
        _appointmentMapper = appointmentMapper;
        _accounts = accounts;
        _notifications = notifications;
        _chartAccess = chartAccess;
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok((await _repo.GetAllAsync()).Select(ToDto));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var vet = await _repo.GetByIdAsync(id);
        return vet is null ? NotFound() : Ok(ToDto(vet));
    }

    [HttpGet("by-clinic/{clinicId}")]
    public async Task<IActionResult> GetByClinic(string clinicId)
        => Ok((await _repo.GetByClinicIdAsync(clinicId)).Select(ToDto));

    /// <summary>
    /// Fișa medicului autentificat, împreună cu cabinetele în care lucrează.
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = AppRoles.Vet)]
    public async Task<IActionResult> GetMe()
    {
        var vet = await _repo.GetByUserIdAsync(CurrentUserId ?? string.Empty);
        if (vet is null)
            return NotFound(new { error = "Contul nu este legat de nicio fișă de medic." });

        var clinics = new List<VetClinicDto>();
        foreach (var clinicId in vet.ClinicIds)
        {
            var clinic = await _clinicRepo.GetByIdAsync(clinicId);
            if (clinic is not null)
            {
                clinics.Add(new VetClinicDto
                {
                    Id = clinic.Id,
                    Name = clinic.Name,
                    Address = clinic.Address,
                    Phone = clinic.Phone
                });
            }
        }

        return Ok(new VetProfileDto
        {
            Id = vet.Id,
            FirstName = vet.FirstName,
            LastName = vet.LastName,
            Email = vet.Email,
            Phone = vet.Phone,
            Specialization = vet.Specialization,
            Clinics = clinics
        });
    }

    /// <summary>
    /// Agenda proprie a medicului autentificat.
    /// </summary>
    [HttpGet("me/appointments")]
    [Authorize(Roles = AppRoles.Vet)]
    public async Task<IActionResult> GetMyAppointments()
    {
        var vet = await _repo.GetByUserIdAsync(CurrentUserId ?? string.Empty);
        if (vet is null)
            return NotFound(new { error = "Contul nu este legat de nicio fișă de medic." });

        var appointments = await _appointmentRepo.GetAllByVetIdAsync(vet.Id);
        var details = await _appointmentMapper.ToDtosAsync(appointments);
        return Ok(details.OrderBy(a => a.DateTime));
    }

    /// <summary>
    /// Un pacient al medicului, cu proprietarul și programările lui. Pagina de fișă
    /// se deschide direct pe acest apel, ca să meargă și pe link, nu doar din agendă.
    /// </summary>
    [HttpGet("me/patients/{petId}")]
    [Authorize(Roles = AppRoles.Vet)]
    public async Task<IActionResult> GetMyPatient(string petId)
    {
        var access = await _chartAccess.ExecuteAsync(petId, CurrentUserId ?? string.Empty, CurrentRoles);
        if (!access.CanRead || access.Pet is null)
            return Forbid();

        var vet = access.Vet ?? await _repo.GetByUserIdAsync(CurrentUserId ?? string.Empty);

        var appointments = vet is null
            ? new List<Appointment>()
            : (await _appointmentRepo.GetAllByVetIdAsync(vet.Id))
                .Where(a => a.PetId == petId)
                .ToList();

        var details = await _appointmentMapper.ToDtosAsync(appointments);
        var owner = await _users.GetContactAsync(access.Pet.OwnerId);
        var pet = access.Pet;

        return Ok(new VetPatientDto
        {
            CanWrite = access.CanWrite,
            Pet = new AppointmentPetDto
            {
                Id = pet.Id,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Age = pet.Age,
                Weight = pet.Weight,
                PhotoUrl = pet.PhotoUrl
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
            },
            Appointments = details.OrderByDescending(a => a.DateTime).ToList()
        });
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> Create(CreateVetDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest(new { error = "Numele și prenumele medicului sunt obligatorii." });

        if (dto.ClinicIds.Count == 0)
            return BadRequest(new { error = "Selectează cel puțin un cabinet." });

        if (!await OwnsAllClinicsAsync(dto.ClinicIds))
            return Forbid();

        var account = await _accounts.CreateAsync(
            dto.FirstName.Trim(), dto.LastName.Trim(), dto.Email?.Trim() ?? string.Empty, dto.Phone?.Trim() ?? string.Empty);

        if (!account.Success)
            return BadRequest(new { error = account.Error });

        var vet = new Vet
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email?.Trim() ?? string.Empty,
            Phone = dto.Phone?.Trim() ?? string.Empty,
            Specialization = dto.Specialization?.Trim() ?? string.Empty,
            ClinicIds = dto.ClinicIds,
            UserId = account.UserId
        };

        try
        {
            await _repo.CreateAsync(vet);
        }
        catch
        {
            // Fără fișa medicului, contul creat ar rămâne orfan.
            await _accounts.DeleteAsync(account.UserId);
            throw;
        }

        await AttachToClinicsAsync(vet);
        await _notifications.VetAccountCreatedAsync(vet, account.Username, account.Password);

        return Ok(new VetAccountDto
        {
            Vet = ToDto(vet),
            Username = account.Username,
            TemporaryPassword = account.Password,
            CredentialsEmailed = !string.IsNullOrWhiteSpace(vet.Email)
        });
    }

    /// <summary>
    /// Creează contul unui medic introdus înainte ca aplicația să genereze conturi.
    /// </summary>
    [HttpPost("{id}/account")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> CreateAccount(string id, [FromBody] CreateVetAccountDto dto)
    {
        var vet = await _repo.GetByIdAsync(id);
        if (vet is null) return NotFound();

        if (!await OwnsAnyClinicAsync(vet.ClinicIds))
            return Forbid();

        if (!string.IsNullOrEmpty(vet.UserId))
            return BadRequest(new { error = "Medicul are deja un cont." });

        var email = string.IsNullOrWhiteSpace(dto.Email) ? vet.Email : dto.Email.Trim();
        var account = await _accounts.CreateAsync(vet.FirstName, vet.LastName, email, vet.Phone);
        if (!account.Success)
            return BadRequest(new { error = account.Error });

        vet.Email = email;
        vet.UserId = account.UserId;

        try
        {
            await _repo.UpdateAsync(vet);
        }
        catch
        {
            await _accounts.DeleteAsync(account.UserId);
            throw;
        }

        await _notifications.VetAccountCreatedAsync(vet, account.Username, account.Password);

        return Ok(new VetAccountDto
        {
            Vet = ToDto(vet),
            Username = account.Username,
            TemporaryPassword = account.Password,
            CredentialsEmailed = !string.IsNullOrWhiteSpace(vet.Email)
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> Update(string id, CreateVetDto dto)
    {
        var vet = await _repo.GetByIdAsync(id);
        if (vet is null) return NotFound();

        if (!await OwnsAnyClinicAsync(vet.ClinicIds))
            return Forbid();

        vet.FirstName = dto.FirstName?.Trim() ?? vet.FirstName;
        vet.LastName = dto.LastName?.Trim() ?? vet.LastName;
        vet.Phone = dto.Phone?.Trim() ?? vet.Phone;
        vet.Specialization = dto.Specialization?.Trim() ?? vet.Specialization;

        await _repo.UpdateAsync(vet);
        return Ok(ToDto(vet));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> Delete(string id)
    {
        var vet = await _repo.GetByIdAsync(id);
        if (vet is null) return NotFound();

        if (!await OwnsAnyClinicAsync(vet.ClinicIds))
            return Forbid();

        await _repo.DeleteAsync(id);

        if (!string.IsNullOrEmpty(vet.UserId))
            await _accounts.DeleteAsync(vet.UserId);

        return NoContent();
    }

    private static VetDto ToDto(Vet vet) => new()
    {
        Id = vet.Id,
        FirstName = vet.FirstName,
        LastName = vet.LastName,
        Email = vet.Email,
        Phone = vet.Phone,
        Specialization = vet.Specialization,
        ClinicIds = vet.ClinicIds,
        HasAccount = !string.IsNullOrEmpty(vet.UserId)
    };

    private async Task<bool> OwnsAllClinicsAsync(IEnumerable<string> clinicIds)
    {
        var owned = (await _clinicRepo.GetByAdminIdAsync(CurrentUserId ?? string.Empty))
            .Select(c => c.Id)
            .ToHashSet();

        return clinicIds.All(owned.Contains);
    }

    private async Task<bool> OwnsAnyClinicAsync(IEnumerable<string> clinicIds)
    {
        var owned = (await _clinicRepo.GetByAdminIdAsync(CurrentUserId ?? string.Empty))
            .Select(c => c.Id)
            .ToHashSet();

        return clinicIds.Any(owned.Contains);
    }

    /// <summary>
    /// Cabinetele își păstrează propria listă de medici, folosită la afișarea publică.
    /// </summary>
    private async Task AttachToClinicsAsync(Vet vet)
    {
        var fullName = $"Dr. {vet.FirstName} {vet.LastName}".Trim();

        foreach (var clinicId in vet.ClinicIds)
        {
            var clinic = await _clinicRepo.GetByIdAsync(clinicId);
            if (clinic is null) continue;

            var changed = false;

            if (!clinic.VetIds.Contains(vet.Id))
            {
                clinic.VetIds.Add(vet.Id);
                changed = true;
            }

            if (!clinic.VetNames.Contains(fullName))
            {
                clinic.VetNames.Add(fullName);
                changed = true;
            }

            if (changed)
                await _clinicRepo.UpdateAsync(clinic);
        }
    }
}
