namespace Somo.Application.DTOs;

/// <summary>
/// Tot ce îi trebuie paginii de pacient a medicului ca să se poată deschide direct
/// pe link, fără să treacă prin agenda din tabloul de bord.
/// </summary>
public class VetPatientDto
{
    public AppointmentPetDto Pet { get; set; } = new();
    public AppointmentOwnerDto? Owner { get; set; }

    /// <summary>
    /// Programările medicului autentificat pentru acest animal, de la cea mai recentă.
    /// </summary>
    public List<AppointmentDetailsDto> Appointments { get; set; } = new();

    /// <summary>
    /// Fals când medicul doar consultă fișa, fără drept de scriere în cabinetul lui.
    /// </summary>
    public bool CanWrite { get; set; }
}
