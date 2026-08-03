using Somo.Application.DTOs;
using Somo.Application.Interfaces;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Infrastructure.Services;

public class ClinicRegistrationService : IClinicRegistrationService
{
    private readonly IVeterinaryClinicRepository _repo;
    private readonly IGooglePlacesService _googlePlacesService;

    public ClinicRegistrationService(
        IVeterinaryClinicRepository repo,
        IGooglePlacesService googlePlacesService)
    {
        _repo = repo;
        _googlePlacesService = googlePlacesService;
    }

    public async Task<VeterinaryClinic> SubmitAsync(RegisterClinicDto dto, string adminId)
    {
        var street = dto.Street.Trim();
        var streetNumber = dto.StreetNumber.Trim();
        var address = string.IsNullOrWhiteSpace(streetNumber)
            ? street
            : $"{street} {streetNumber}";

        var coords = await _googlePlacesService.GeocodeAddressAsync(
            $"{address}, {dto.City.Trim()}, {dto.County.Trim()}, Romania");

        var clinic = new VeterinaryClinic
        {
            AdminId = adminId,
            Name = dto.Name.Trim(),
            Address = address,
            Street = street,
            StreetNumber = streetNumber,
            City = dto.City.Trim(),
            County = dto.County.Trim(),
            Phone = dto.Phone.Trim(),
            Email = dto.Email.Trim(),
            Schedule = dto.Schedule.Trim(),
            VetNames = dto.VetNames
                .Select(v => v.Trim())
                .Where(v => v.Length > 0)
                .ToList(),
            Prices = dto.Prices
                .Where(p => !string.IsNullOrWhiteSpace(p.Service))
                .Select(p => new ClinicPrice { Service = p.Service.Trim(), Price = p.Price })
                .ToList(),
            Latitude = coords?.Lat ?? 0,
            Longitude = coords?.Lng ?? 0,
            VetIds = new List<string>(),
            Status = ClinicStatus.Pending,
            RequestedAtUtc = DateTime.UtcNow
        };

        await _repo.CreateAsync(clinic);
        return clinic;
    }
}
