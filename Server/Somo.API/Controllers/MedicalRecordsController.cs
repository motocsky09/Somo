using Microsoft.AspNetCore.Mvc;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordRepository _repo;

    public MedicalRecordsController(IMedicalRecordRepository repo) => _repo = repo;

    [HttpGet("pet/{petId}")]
    public async Task<IActionResult> GetByPet(string petId)
        => Ok(await _repo.GetAllByPetIdAsync(petId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await _repo.GetByIdAsync(id);
        return record is null ? NotFound() : Ok(record);
    }

    [HttpPost]
    public async Task<IActionResult> Create(MedicalRecord record)
    {
        await _repo.CreateAsync(record);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, MedicalRecord record)
    {
        record.Id = id;
        await _repo.UpdateAsync(record);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}