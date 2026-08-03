using Somo.Application.DTOs;
using Somo.Domain.Entities;

namespace Somo.Application.Interfaces;

public interface IClinicRegistrationService
{
    Task<VeterinaryClinic> SubmitAsync(RegisterClinicDto dto, string adminId);
}
