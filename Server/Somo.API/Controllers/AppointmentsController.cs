using Microsoft.AspNetCore.Mvc;
using Somo.Application.DTOs;
using Somo.Application.Features.Appointments.Commands;
using Somo.Application.Features.Appointments.Queries;
using Somo.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _repo;
    private readonly CreateAppointmentCommand _createCommand;
    private readonly GetAvailableSlotsQuery _slotsQuery;

    public AppointmentsController(
        IAppointmentRepository repo,
        CreateAppointmentCommand createCommand,
        GetAvailableSlotsQuery slotsQuery)
    {
        _repo = repo;
        _createCommand = createCommand;
        _slotsQuery = slotsQuery;
    }

    [HttpGet("owner/{ownerId}")]
    public async Task<IActionResult> GetByOwner(string ownerId)
        => Ok(await _repo.GetAllByOwnerIdAsync(ownerId));

    [HttpGet("vet/{vetId}")]
    public async Task<IActionResult> GetByVet(string vetId)
        => Ok(await _repo.GetAllByVetIdAsync(vetId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var appointment = await _repo.GetByIdAsync(id);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpGet("available-slots")]
    public async Task<IActionResult> GetAvailableSlots(string vetId, DateTime date)
        => Ok(await _slotsQuery.ExecuteAsync(vetId, date));

    [HttpPost]
    [Authorize] 
    public async Task<IActionResult> Create(CreateAppointmentDto dto)
    {
    
    var ownerId = User.FindFirst("id")?.Value;

    if (string.IsNullOrEmpty(ownerId))
        return Unauthorized(new { error = "Token invalid." });

    var (success, error) = await _createCommand.ExecuteAsync(dto, ownerId);

    if (!success) return BadRequest(new { error });
    return Ok(new { message = "Programare creată cu succes." });
}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}