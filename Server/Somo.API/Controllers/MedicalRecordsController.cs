using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Somo.Application.DTOs;
using Somo.Application.Features.Medical;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalRecordsController : ApiControllerBase
{
    private readonly IMedicalRecordRepository _repo;
    private readonly IPetRepository _petRepo;
    private readonly PetChartAccessQuery _access;
    private readonly MedicalRecordMapper _mapper;

    public MedicalRecordsController(
        IMedicalRecordRepository repo,
        IPetRepository petRepo,
        PetChartAccessQuery access,
        MedicalRecordMapper mapper)
    {
        _repo = repo;
        _petRepo = petRepo;
        _access = access;
        _mapper = mapper;
    }

    [HttpGet("pet/{petId}")]
    public async Task<IActionResult> GetByPet(string petId)
    {
        var access = await _access.ExecuteAsync(petId, CurrentUserId ?? string.Empty, CurrentRoles);
        if (!access.CanRead)
            return Forbid();

        var records = await _repo.GetAllByPetIdAsync(petId);
        return Ok(await _mapper.ToDtosAsync(records));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await _repo.GetByIdAsync(id);
        if (record is null) return NotFound();

        var access = await _access.ExecuteAsync(record.PetId, CurrentUserId ?? string.Empty, CurrentRoles);
        if (!access.CanRead)
            return Forbid();

        var dtos = await _mapper.ToDtosAsync(new[] { record });
        return Ok(dtos.Single());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMedicalRecordDto dto)
    {
        var access = await _access.ExecuteAsync(dto.PetId, CurrentUserId ?? string.Empty, CurrentRoles);
        if (!access.CanWrite || access.Vet is null || access.Pet is null)
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Diagnosis) && string.IsNullOrWhiteSpace(dto.Treatment))
            return BadRequest(new { error = "Completează cel puțin diagnosticul sau tratamentul." });

        var record = new MedicalRecord
        {
            PetId = dto.PetId,
            OwnerId = access.Pet.OwnerId,
            VetId = access.Vet.Id,
            ClinicId = access.ClinicId,
            AppointmentId = dto.AppointmentId,
            Date = dto.Date ?? DateTime.UtcNow,
            Diagnosis = dto.Diagnosis?.Trim() ?? string.Empty,
            Treatment = dto.Treatment?.Trim() ?? string.Empty,
            Notes = dto.Notes?.Trim() ?? string.Empty,
            Weight = dto.Weight,
            Temperature = dto.Temperature
        };

        await _repo.CreateAsync(record);
        await SyncPetWeightAsync(access.Pet, record);

        var dtos = await _mapper.ToDtosAsync(new[] { record });
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, dtos.Single());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateMedicalRecordDto dto)
    {
        var record = await _repo.GetByIdAsync(id);
        if (record is null) return NotFound();

        var access = await _access.ExecuteAsync(record.PetId, CurrentUserId ?? string.Empty, CurrentRoles);
        if (!access.CanWrite || access.Vet is null || access.Pet is null)
            return Forbid();

        if (record.VetId != access.Vet.Id)
            return Forbid();

        record.Date = dto.Date ?? record.Date;
        record.Diagnosis = dto.Diagnosis?.Trim() ?? string.Empty;
        record.Treatment = dto.Treatment?.Trim() ?? string.Empty;
        record.Notes = dto.Notes?.Trim() ?? string.Empty;
        record.Weight = dto.Weight;
        record.Temperature = dto.Temperature;

        await _repo.UpdateAsync(record);
        await SyncPetWeightAsync(access.Pet, record);

        var dtos = await _mapper.ToDtosAsync(new[] { record });
        return Ok(dtos.Single());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var record = await _repo.GetByIdAsync(id);
        if (record is null) return NotFound();

        var access = await _access.ExecuteAsync(record.PetId, CurrentUserId ?? string.Empty, CurrentRoles);
        if (!access.CanWrite || access.Vet is null || record.VetId != access.Vet.Id)
            return Forbid();

        await _repo.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Greutatea de pe fișa animalului este cea de la ultima vizită cântărită.
    /// </summary>
    private async Task SyncPetWeightAsync(Pet pet, MedicalRecord record)
    {
        if (record.Weight <= 0)
            return;

        var records = await _repo.GetAllByPetIdAsync(pet.Id);
        var latest = records
            .Where(r => r.Weight > 0)
            .OrderByDescending(r => r.Date)
            .FirstOrDefault();

        if (latest is null || Math.Abs(pet.Weight - latest.Weight) < 0.001)
            return;

        pet.Weight = latest.Weight;
        await _petRepo.UpdateAsync(pet);
    }
}
