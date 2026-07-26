using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public class PetsController : ControllerBase
{
    private readonly IPetRepository _repo;

    public PetsController(IPetRepository repo) => _repo = repo;

    private string? CurrentUserId => User.FindFirst("id")?.Value;

    [HttpGet("owner/{ownerId}")]
    public async Task<IActionResult> GetByOwner(string ownerId)
    {
        if (ownerId != CurrentUserId)
            return Forbid();

        return Ok(await _repo.GetAllByOwnerIdAsync(ownerId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var (pet, error) = await GetOwnedPetAsync(id);
        return error ?? Ok(pet);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Pet pet)
    {
        pet.OwnerId = CurrentUserId ?? string.Empty;
        await _repo.CreateAsync(pet);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Pet pet)
    {
        var (existing, error) = await GetOwnedPetAsync(id);
        if (error is not null)
            return error;

        existing!.Name = pet.Name;
        existing.Species = pet.Species;
        existing.Breed = pet.Breed;
        existing.Age = pet.Age;
        existing.Weight = pet.Weight;
        existing.PhotoUrl = pet.PhotoUrl;

        await _repo.UpdateAsync(existing);
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var (_, error) = await GetOwnedPetAsync(id);
        if (error is not null)
            return error;

        await _repo.DeleteAsync(id);
        return NoContent();
    }

    private async Task<(Pet? pet, IActionResult? error)> GetOwnedPetAsync(string id)
    {
        var pet = await _repo.GetByIdAsync(id);
        if (pet is null)
            return (null, NotFound());
        if (pet.OwnerId != CurrentUserId)
            return (null, Forbid());

        return (pet, null);
    }
}
