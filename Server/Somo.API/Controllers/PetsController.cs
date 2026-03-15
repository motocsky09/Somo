using Microsoft.AspNetCore.Mvc;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly IPetRepository _repo;

    public PetsController(IPetRepository repo) => _repo = repo;

    [HttpGet("owner/{ownerId}")]
    public async Task<IActionResult> GetByOwner(string ownerId)
        => Ok(await _repo.GetAllByOwnerIdAsync(ownerId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var pet = await _repo.GetByIdAsync(id);
        return pet is null ? NotFound() : Ok(pet);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Pet pet)
    {
        await _repo.CreateAsync(pet);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Pet pet)
    {
        pet.Id = id;
        await _repo.UpdateAsync(pet);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}