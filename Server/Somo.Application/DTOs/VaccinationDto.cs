namespace Somo.Application.DTOs;

public class VaccinationDto
{
    public string Id { get; set; } = string.Empty;
    public string PetId { get; set; } = string.Empty;
    public string VetId { get; set; } = string.Empty;
    public string ClinicId { get; set; } = string.Empty;
    public string VaccineCode { get; set; } = string.Empty;
    public string VaccineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime AdministeredOn { get; set; }
    public DateTime NextDueOn { get; set; }
    public bool ReminderSent { get; set; }

    public string VetName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;

    /// <summary>
    /// Câte zile mai sunt până la rapel; negativ înseamnă rapel depășit.
    /// </summary>
    public int DaysUntilDue { get; set; }
}

public class CreateVaccinationDto
{
    public string PetId { get; set; } = string.Empty;
    public string VaccineCode { get; set; } = string.Empty;
    public DateTime AdministeredOn { get; set; }

    /// <summary>
    /// Opțional. Când lipsește, se calculează din intervalul din catalog.
    /// </summary>
    public DateTime? NextDueOn { get; set; }

    public string BatchNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class UpdateVaccinationDto
{
    public string VaccineCode { get; set; } = string.Empty;
    public DateTime AdministeredOn { get; set; }

    /// <summary>
    /// Opțional. Când lipsește, se recalculează din intervalul din catalog.
    /// </summary>
    public DateTime? NextDueOn { get; set; }

    public string BatchNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class VaccineTypeDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public int IntervalMonths { get; set; }
    public bool IsMandatory { get; set; }
    public string Description { get; set; } = string.Empty;
}
