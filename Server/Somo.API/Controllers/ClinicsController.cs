using Microsoft.AspNetCore.Mvc;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Somo.Application.Interfaces;
using Somo.Application.DTOs;
using Somo.Application.Common;

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

    private string CurrentUserId => User.FindFirst("id")?.Value ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _repo.GetApprovedAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var clinic = await _repo.GetByIdAsync(id);
        if (clinic is null)
            return NotFound();

        var canSeeUnapproved = clinic.AdminId == CurrentUserId || User.IsInRole(AppRoles.SomoAdmin);
        if (clinic.Status != ClinicStatus.Approved && !canSeeUnapproved)
            return NotFound();

        return Ok(clinic);
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

    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> Update(string id, VeterinaryClinic clinic)
    {
        var stored = await _repo.GetByIdAsync(id);
        if (stored is null)
            return NotFound();

        if (stored.AdminId != CurrentUserId)
            return Forbid();

        stored.Name = clinic.Name;
        stored.Phone = clinic.Phone;
        stored.Email = clinic.Email;
        stored.Schedule = clinic.Schedule;
        stored.VetNames = clinic.VetNames;
        stored.Prices = clinic.Prices;

        await _repo.UpdateAsync(stored);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> Delete(string id)
    {
        var stored = await _repo.GetByIdAsync(id);
        if (stored is null)
            return NotFound();

        if (stored.AdminId != CurrentUserId)
            return Forbid();

        await _repo.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("register")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> Register(
        RegisterClinicDto dto,
        [FromServices] IClinicRegistrationService clinicRegistration)
    {
        var clinic = await clinicRegistration.SubmitAsync(dto, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = clinic.Id }, clinic);
    }

    [HttpGet("my-clinics")]
    [Authorize(Roles = AppRoles.ClinicAdmin)]
    public async Task<IActionResult> GetMyClinics()
    {
        var clinics = await _repo.GetByAdminIdAsync(CurrentUserId);

        return Ok(clinics.Select(c => new
        {
            c.Id,
            c.Name,
            c.Address,
            c.Street,
            c.StreetNumber,
            c.City,
            c.County,
            c.Phone,
            c.Email,
            c.Schedule,
            c.VetNames,
            c.Prices,
            c.VetIds,
            c.Latitude,
            c.Longitude,
            c.RejectionReason,
            c.RequestedAtUtc,
            c.ReviewedAtUtc,
            status = c.Status.ToString()
        }));
    }
}
