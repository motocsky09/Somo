using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Somo.API.Entities;
using Somo.API.Models;
using Somo.Application.Common;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.SomoAdmin)]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IVeterinaryClinicRepository _clinicRepo;
    private readonly IPetRepository _petRepo;
    private readonly IVetRepository _vetRepo;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        IVeterinaryClinicRepository clinicRepo,
        IPetRepository petRepo,
        IVetRepository vetRepo)
    {
        _userManager = userManager;
        _clinicRepo = clinicRepo;
        _petRepo = petRepo;
        _vetRepo = vetRepo;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var clinics = (await _clinicRepo.GetAllAsync()).ToList();
        var owners = await _userManager.GetUsersInRoleAsync(AppRoles.Owner);
        var clinicAdmins = await _userManager.GetUsersInRoleAsync(AppRoles.ClinicAdmin);

        return Ok(new
        {
            owners = owners.Count,
            clinicAdmins = clinicAdmins.Count,
            clinicsTotal = clinics.Count,
            clinicsPending = clinics.Count(c => c.Status == ClinicStatus.Pending),
            clinicsApproved = clinics.Count(c => c.Status == ClinicStatus.Approved),
            clinicsRejected = clinics.Count(c => c.Status == ClinicStatus.Rejected)
        });
    }

    [HttpGet("owners")]
    public async Task<IActionResult> GetOwners()
    {
        var owners = await _userManager.GetUsersInRoleAsync(AppRoles.Owner);
        var result = new List<object>();

        foreach (var owner in owners)
        {
            var pets = await _petRepo.GetAllByOwnerIdAsync(owner.Id.ToString());
            result.Add(new
            {
                id = owner.Id.ToString(),
                username = owner.UserName,
                email = owner.Email,
                pets = pets.Select(p => new { p.Id, p.Name, p.Species, p.Breed, p.Age })
            });
        }

        return Ok(result);
    }

    [HttpGet("clinics")]
    public async Task<IActionResult> GetClinics()
    {
        var clinics = await _clinicRepo.GetAllAsync();
        var result = new List<object>();

        foreach (var clinic in clinics.OrderByDescending(c => c.RequestedAtUtc))
            result.Add(await DescribeClinicAsync(clinic));

        return Ok(result);
    }

    [HttpPost("clinics/{id}/approve")]
    public async Task<IActionResult> ApproveClinic(string id)
    {
        var clinic = await _clinicRepo.GetByIdAsync(id);
        if (clinic is null)
            return NotFound();

        if (clinic.Status == ClinicStatus.Approved)
            return Ok(await DescribeClinicAsync(clinic));

        foreach (var vetId in await CreateDeclaredVetsAsync(clinic))
            clinic.VetIds.Add(vetId);

        clinic.Status = ClinicStatus.Approved;
        clinic.RejectionReason = string.Empty;
        clinic.ReviewedAtUtc = DateTime.UtcNow;

        await _clinicRepo.UpdateAsync(clinic);
        return Ok(await DescribeClinicAsync(clinic));
    }

    [HttpPost("clinics/{id}/reject")]
    public async Task<IActionResult> RejectClinic(string id, [FromBody] RejectClinicModel model)
    {
        var clinic = await _clinicRepo.GetByIdAsync(id);
        if (clinic is null)
            return NotFound();

        clinic.Status = ClinicStatus.Rejected;
        clinic.RejectionReason = model.Reason?.Trim() ?? string.Empty;
        clinic.ReviewedAtUtc = DateTime.UtcNow;

        await _clinicRepo.UpdateAsync(clinic);
        return Ok(await DescribeClinicAsync(clinic));
    }

    [HttpDelete("clinics/{id}")]
    public async Task<IActionResult> DeleteClinic(string id)
    {
        var clinic = await _clinicRepo.GetByIdAsync(id);
        if (clinic is null)
            return NotFound();

        await _clinicRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await FindUserAsync(id);
        if (user is null)
            return NotFound();

        if (await _userManager.IsInRoleAsync(user, AppRoles.SomoAdmin))
            return BadRequest(new { message = "Conturile de administrator Somo nu pot fi șterse." });

        foreach (var pet in await _petRepo.GetAllByOwnerIdAsync(id))
            await _petRepo.DeleteAsync(pet.Id);

        foreach (var clinic in await _clinicRepo.GetByAdminIdAsync(id))
            await _clinicRepo.DeleteAsync(clinic.Id);

        await _userManager.DeleteAsync(user);
        return NoContent();
    }

    private async Task<List<string>> CreateDeclaredVetsAsync(VeterinaryClinic clinic)
    {
        var existing = (await _vetRepo.GetAllAsync())
            .Where(v => v.ClinicIds.Contains(clinic.Id))
            .Select(v => $"{v.FirstName} {v.LastName}".Trim().ToLowerInvariant())
            .ToHashSet();

        var created = new List<string>();

        foreach (var name in clinic.VetNames)
        {
            var cleaned = name.Trim();
            if (cleaned.Length == 0 || existing.Contains(cleaned.ToLowerInvariant()))
                continue;

            var parts = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var vet = new Vet
            {
                FirstName = parts[0],
                LastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : string.Empty,
                Email = clinic.Email,
                Phone = clinic.Phone,
                Specialization = "Medic veterinar",
                ClinicIds = new List<string> { clinic.Id }
            };

            await _vetRepo.CreateAsync(vet);
            created.Add(vet.Id);
        }

        return created;
    }

    private async Task<ApplicationUser?> FindUserAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            return null;

        return await _userManager.FindByIdAsync(id);
    }

    private async Task<object> DescribeClinicAsync(VeterinaryClinic clinic)
    {
        var admin = await FindUserAsync(clinic.AdminId);

        return new
        {
            clinic.Id,
            clinic.Name,
            clinic.Address,
            clinic.Street,
            clinic.StreetNumber,
            clinic.City,
            clinic.County,
            clinic.Phone,
            clinic.Email,
            clinic.Schedule,
            clinic.VetNames,
            clinic.Prices,
            clinic.Latitude,
            clinic.Longitude,
            clinic.RejectionReason,
            clinic.RequestedAtUtc,
            clinic.ReviewedAtUtc,
            status = clinic.Status.ToString(),
            adminId = clinic.AdminId,
            adminUsername = admin?.UserName,
            adminEmail = admin?.Email
        };
    }
}
