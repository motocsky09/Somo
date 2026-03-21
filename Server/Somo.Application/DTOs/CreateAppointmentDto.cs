namespace Somo.Application.DTOs;

public class CreateAppointmentDto
{
    public string PetId { get; set; } = string.Empty;
    public string VetId { get; set; } = string.Empty;
    public string ClinicId { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string Reason { get; set; } = string.Empty;
}