using Microsoft.AspNetCore.Mvc;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicsController : ControllerBase
{
    private readonly IVeterinaryClinicRepository _repo;

    public ClinicsController(IVeterinaryClinicRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var clinic = await _repo.GetByIdAsync(id);
        return clinic is null ? NotFound() : Ok(clinic);
    }

    [HttpGet("city/{city}")]
    public async Task<IActionResult> GetByCity(string city)
        => Ok(await _repo.GetByCityAsync(city));

    [HttpPost]
    [Authorize(Roles = "ClinicAdmin")]
    public async Task<IActionResult> Create(VeterinaryClinic clinic)
    {
        await _repo.CreateAsync(clinic);
        return CreatedAtAction(nameof(GetById), new { id = clinic.Id }, clinic);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ClinicAdmin")]
    public async Task<IActionResult> Update(string id, VeterinaryClinic clinic)
    {
        clinic.Id = id;
        await _repo.UpdateAsync(clinic);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ClinicAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby(double lat, double lng, double radiusKm = 10)
        => Ok(await _repo.GetNearbyAsync(lat, lng, radiusKm));
}