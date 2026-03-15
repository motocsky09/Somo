using Microsoft.AspNetCore.Mvc;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _repo;

    public AppointmentsController(IAppointmentRepository repo) => _repo = repo;

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

    [HttpPost]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        await _repo.CreateAsync(appointment);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Appointment appointment)
    {
        appointment.Id = id;
        await _repo.UpdateAsync(appointment);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}