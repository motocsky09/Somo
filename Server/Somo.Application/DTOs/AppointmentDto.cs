namespace Somo.Application.DTOs;

public class AppointmentDto
{
    public string Id { get; set; } = string.Empty;
    public string PetName { get; set; } = string.Empty;
    public string VetName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}