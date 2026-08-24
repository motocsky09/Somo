using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Somo.Application.DTOs;
using Somo.Application.Features.Medical;
using Somo.Application.Features.Vaccinations;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VaccinationsController : ApiControllerBase
{
    private readonly IVaccinationRepository _repo;
    private readonly PetChartAccessQuery _access;
    private readonly VaccinationMapper _mapper;

    public VaccinationsController(
        IVaccinationRepository repo,
        PetChartAccessQuery access,
        VaccinationMapper mapper)
    {
        _repo = repo;
        _access = access;
        _mapper = mapper;
    }

    /// <summary>
    /// Schema de vaccinare din care alege medicul. Filtrabilă după specie.
    /// </summary>
    [HttpGet("catalog")]
    [AllowAnonymous]
    public IActionResult GetCatalog([FromQuery] string? species)
    {
        var types = string.IsNullOrWhiteSpace(species)
            ? VaccineCatalog.All
            : VaccineCatalog.ForSpecies(species).ToList();

        return Ok(types.Select(t => new VaccineTypeDto
        {
            Code = t.Code,
            Name = t.Name,
            Species = t.Species,
            IntervalMonths = t.IntervalMonths,
            IsMandatory = t.IsMandatory,
            Description = t.Description
        }));
    }

    [HttpGet("pet/{petId}")]
    public async Task<IActionResult> GetByPet(string petId)
    {
        var access = await _access.ExecuteAsync(petId, CurrentUserId ?? string.Empty, CurrentRoles);
        if (!access.CanRead)
            return Forbid();

        var vaccinations = await _repo.GetAllByPetIdAsync(petId);
        return Ok(await _mapper.ToDtosAsync(vaccinations));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var vaccination = await _repo.GetByIdAsync(id);
        if (vaccination is null) return NotFound();

        var access = await _access.ExecuteAsync(vaccination.PetId, CurrentUserId ?? string.Empty, CurrentRoles);
        if (!access.CanRead)
            return Forbid();

        var dtos = await _mapper.ToDtosAsync(new[] { vaccination });
        return Ok(dtos.Single());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVaccinationDto dto)
    {
        var access = await _access.ExecuteAsync(dto.PetId, CurrentUserId ?? string.Empty, CurrentRoles);
        if (!access.CanWrite || access.Vet is null || access.Pet is null)
            return Forbid();

        var type = VaccineCatalog.Find(dto.VaccineCode);
        if (type is null)
            return BadRequest(new { error = "Vaccinul selectat nu există în schema de vaccinare." });

        var administeredOn = dto.AdministeredOn == default
            ? DateTime.UtcNow.Date
            : AsUtcDate(dto.AdministeredOn);

        if (administeredOn > DateTime.UtcNow.Date)
            return BadRequest(new { error = "Data administrării nu poate fi în viitor." });

        var nextDueOn = dto.NextDueOn is null
            ? VaccineCatalog.NextDueDate(type.Code, administeredOn)
            : AsUtcDate(dto.NextDueOn.Value);
        if (nextDueOn <= administeredOn)
            return BadRequest(new { error = "Data rapelului trebuie să fie după data administrării." });

        var vaccination = new Vaccination
        {
            PetId = dto.PetId,
            OwnerId = access.Pet.OwnerId,
            VetId = access.Vet.Id,
            ClinicId = access.ClinicId,
            VaccineCode = type.Code,
            VaccineName = type.Name,
            BatchNumber = dto.BatchNumber?.Trim() ?? string.Empty,
            Notes = dto.Notes?.Trim() ?? string.Empty,
            AdministeredOn = administeredOn,
            NextDueOn = nextDueOn
        };

        await _repo.CreateAsync(vaccination);

        var dtos = await _mapper.ToDtosAsync(new[] { vaccination });
        return CreatedAtAction(nameof(GetById), new { id = vaccination.Id }, dtos.Single());
    }

    /// <summary>
    /// Un vaccin este datat cu ziua, nu cu ora. Fixăm miezul nopții în UTC ca data
    /// să nu alunece cu o zi la trecerea prin bază și înapoi.
    /// </summary>
    private static DateTime AsUtcDate(DateTime value)
        => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateVaccinationDto dto)
    {
        var vaccination = await _repo.GetByIdAsync(id);
        if (vaccination is null) return NotFound();

        var access = await _access.ExecuteAsync(vaccination.PetId, CurrentUserId ?? string.Empty, CurrentRoles);
        if (!access.CanWrite || access.Vet is null || vaccination.VetId != access.Vet.Id)
            return Forbid();

        var type = VaccineCatalog.Find(dto.VaccineCode);
        if (type is null)
            return BadRequest(new { error = "Vaccinul selectat nu există în schema de vaccinare." });

        var administeredOn = dto.AdministeredOn == default
            ? vaccination.AdministeredOn
            : AsUtcDate(dto.AdministeredOn);

        if (administeredOn > DateTime.UtcNow.Date)
            return BadRequest(new { error = "Data administrării nu poate fi în viitor." });

        var nextDueOn = dto.NextDueOn is null
            ? VaccineCatalog.NextDueDate(type.Code, administeredOn)
            : AsUtcDate(dto.NextDueOn.Value);
        if (nextDueOn <= administeredOn)
            return BadRequest(new { error = "Data rapelului trebuie să fie după data administrării." });

        // Rapelul mutat în viitor merită un reminder nou, chiar dacă cel vechi a plecat deja.
        if (nextDueOn != vaccination.NextDueOn)
            vaccination.ReminderSentAtUtc = null;

        vaccination.VaccineCode = type.Code;
        vaccination.VaccineName = type.Name;
        vaccination.AdministeredOn = administeredOn;
        vaccination.NextDueOn = nextDueOn;
        vaccination.BatchNumber = dto.BatchNumber?.Trim() ?? string.Empty;
        vaccination.Notes = dto.Notes?.Trim() ?? string.Empty;

        await _repo.UpdateAsync(vaccination);

        var dtos = await _mapper.ToDtosAsync(new[] { vaccination });
        return Ok(dtos.Single());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var vaccination = await _repo.GetByIdAsync(id);
        if (vaccination is null) return NotFound();

        var access = await _access.ExecuteAsync(vaccination.PetId, CurrentUserId ?? string.Empty, CurrentRoles);
        if (!access.CanWrite || access.Vet is null || vaccination.VetId != access.Vet.Id)
            return Forbid();

        await _repo.DeleteAsync(id);
        return NoContent();
    }
}
