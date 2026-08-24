using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Somo.Application.Common;
using Somo.Application.DTOs;
using Somo.Application.Features.Appointments.Commands;
using Somo.Application.Features.Appointments.Queries;
using Somo.Application.Interfaces;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ApiControllerBase
{
    private readonly IAppointmentRepository _repo;
    private readonly IVeterinaryClinicRepository _clinicRepo;
    private readonly IVetRepository _vetRepo;
    private readonly AppointmentDetailsMapper _detailsMapper;
    private readonly CreateAppointmentCommand _createCommand;
    private readonly GetAvailableSlotsQuery _slotsQuery;
    private readonly INotificationService _notifications;

    public AppointmentsController(
        IAppointmentRepository repo,
        IVeterinaryClinicRepository clinicRepo,
        IVetRepository vetRepo,
        AppointmentDetailsMapper detailsMapper,
        CreateAppointmentCommand createCommand,
        GetAvailableSlotsQuery slotsQuery,
        INotificationService notifications)
    {
        _repo = repo;
        _clinicRepo = clinicRepo;
        _vetRepo = vetRepo;
        _detailsMapper = detailsMapper;
        _createCommand = createCommand;
        _slotsQuery = slotsQuery;
        _notifications = notifications;
    }

    [HttpGet("owner/{ownerId}")]
    public async Task<IActionResult> GetByOwner(string ownerId)
    {
        if (ownerId != CurrentUserId)
            return Forbid();

        return Ok(await _repo.GetAllByOwnerIdAsync(ownerId));
    }

    [HttpGet("vet/{vetId}")]
    public async Task<IActionResult> GetByVet(string vetId)
    {
        var vet = await _vetRepo.GetByIdAsync(vetId);
        if (vet is null) return NotFound();

        if (vet.UserId != CurrentUserId && !await OwnsAnyClinicAsync(vet.ClinicIds))
            return Forbid();

        return Ok(await _repo.GetAllByVetIdAsync(vetId));
    }

    [HttpGet("by-clinic/{clinicId}")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> GetByClinic(string clinicId)
    {
        var clinic = await _clinicRepo.GetByIdAsync(clinicId);
        if (clinic is null) return NotFound();
        if (clinic.AdminId != CurrentUserId) return Forbid();

        var appointments = await _repo.GetByClinicIdAsync(clinicId);
        return Ok(await _detailsMapper.ToDtosAsync(appointments));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var appointment = await _repo.GetByIdAsync(id);
        if (appointment is null) return NotFound();

        if (!await CanSeeAsync(appointment)) return Forbid();

        return Ok(appointment);
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetDetails(string id)
    {
        var appointment = await _repo.GetByIdAsync(id);
        if (appointment is null) return NotFound();

        if (!await CanSeeAsync(appointment)) return Forbid();

        var details = await _detailsMapper.ToDtosAsync(new[] { appointment });
        return Ok(details.Single());
    }

    [HttpGet("available-slots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSlots(string vetId, DateTime date)
        => Ok(await _slotsQuery.ExecuteAsync(vetId, date));

    [HttpPost]
    public async Task<IActionResult> Create(CreateAppointmentDto dto)
    {
        var ownerId = CurrentUserId;
        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized(new { error = "Token invalid." });

        var (success, error) = await _createCommand.ExecuteAsync(dto, ownerId);
        if (!success) return BadRequest(new { error });
        return Ok(new { message = "Programare creată cu succes." });
    }

    /// <summary>
    /// Cabinetul și medicul pot schimba orice stare; proprietarul își poate doar
    /// anula propria programare.
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] int status)
    {
        var appointment = await _repo.GetByIdAsync(id);
        if (appointment is null) return NotFound();

        if (!Enum.IsDefined(typeof(AppointmentStatus), status))
            return BadRequest(new { error = "Stare invalidă." });

        var newStatus = (AppointmentStatus)status;
        var isStaff = await CanManageAsync(appointment);

        if (!isStaff)
        {
            if (appointment.OwnerId != CurrentUserId)
                return Forbid();

            if (newStatus != AppointmentStatus.Cancelled)
                return Forbid();
        }

        var previousStatus = appointment.Status;
        if (previousStatus == newStatus)
            return NoContent();

        appointment.Status = newStatus;
        await _repo.UpdateAsync(appointment);

        if (newStatus == AppointmentStatus.Confirmed)
            await _notifications.AppointmentConfirmedAsync(appointment);
        else if (newStatus == AppointmentStatus.Cancelled)
            await _notifications.AppointmentCancelledAsync(appointment);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var appointment = await _repo.GetByIdAsync(id);
        if (appointment is null) return NotFound();

        if (appointment.OwnerId != CurrentUserId && !await CanManageAsync(appointment))
            return Forbid();

        await _repo.DeleteAsync(id);
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> Update(string id, Appointment appointment)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null) return NotFound();
        if (!await CanManageAsync(existing)) return Forbid();

        appointment.Id = id;
        await _repo.UpdateAsync(appointment);
        return NoContent();
    }

    private async Task<bool> CanSeeAsync(Appointment appointment)
        => appointment.OwnerId == CurrentUserId || await CanManageAsync(appointment);

    /// <summary>
    /// Cabinetul care găzduiește programarea sau medicul căruia îi este atribuită.
    /// </summary>
    private async Task<bool> CanManageAsync(Appointment appointment)
    {
        var clinic = await _clinicRepo.GetByIdAsync(appointment.ClinicId);
        if (clinic is not null && clinic.AdminId == CurrentUserId)
            return true;

        var vet = await _vetRepo.GetByIdAsync(appointment.VetId);
        return vet is not null &&
               !string.IsNullOrEmpty(vet.UserId) &&
               vet.UserId == CurrentUserId;
    }

    private async Task<bool> OwnsAnyClinicAsync(IEnumerable<string> clinicIds)
    {
        var owned = (await _clinicRepo.GetByAdminIdAsync(CurrentUserId ?? string.Empty))
            .Select(c => c.Id)
            .ToHashSet();

        return clinicIds.Any(owned.Contains);
    }
}
