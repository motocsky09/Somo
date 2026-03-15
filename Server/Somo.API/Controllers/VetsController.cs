using Microsoft.AspNetCore.Mvc;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VetsController : ControllerBase
{
    private readonly IVetRepository _repo;

    public VetsController(IVetRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var vet = await _repo.GetByIdAsync(id);
        return vet is null ? NotFound() : Ok(vet);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Vet vet)
    {
        await _repo.CreateAsync(vet);
        return CreatedAtAction(nameof(GetById), new { id = vet.Id }, vet);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Vet vet)
    {
        vet.Id = id;
        await _repo.UpdateAsync(vet);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}