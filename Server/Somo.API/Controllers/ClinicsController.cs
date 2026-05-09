using Microsoft.AspNetCore.Mvc;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Somo.Application.Interfaces;
using Somo.Application.DTOs;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicsController : ControllerBase
{
    private readonly IVeterinaryClinicRepository _repo;
    private readonly IGooglePlacesService _googlePlacesService;

    public ClinicsController(
        IVeterinaryClinicRepository repo,
        IGooglePlacesService googlePlacesService)
    {
        _repo = repo;
        _googlePlacesService = googlePlacesService;
    }

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

    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby(double lat, double lng, double radiusKm = 10)
    {
        var dbClinics = await _repo.GetNearbyAsync(lat, lng, radiusKm);
        var googleClinics = await _googlePlacesService.SearchVeterinaryClinicsAsync(
            lat, lng, radiusKm * 1000);

        var dbClinicNames = dbClinics.Select(c => c.Name.ToLower()).ToHashSet();
        foreach (var gc in googleClinics)
        {
            gc.IsInDatabase = dbClinicNames.Contains(gc.Name.ToLower());
        }

        return Ok(new
        {
            databaseClinics = dbClinics,
            googleClinics = googleClinics
        });
    }

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

    [HttpPost("register")]
[Authorize(Roles = "ClinicAdmin")]
    public async Task<IActionResult> Register(
        RegisterClinicDto dto,
        [FromServices] IGooglePlacesService googlePlacesService)
    {
        // Geocodează adresa automat
        var coords = await googlePlacesService.GeocodeAddressAsync(
            $"{dto.Address}, {dto.City}, Romania");

        var clinic = new VeterinaryClinic
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            Phone = dto.Phone,
            Email = dto.Email,
            Schedule = dto.Schedule,
            Latitude = coords?.Lat ?? 0,
            Longitude = coords?.Lng ?? 0,
            VetIds = new List<string>()
        };

        await _repo.CreateAsync(clinic);
        return CreatedAtAction(nameof(GetById), new { id = clinic.Id }, clinic);
    }
}
