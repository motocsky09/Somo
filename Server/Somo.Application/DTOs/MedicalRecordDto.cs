namespace Somo.Application.DTOs;

public class MedicalRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string PetId { get; set; } = string.Empty;
    public string VetId { get; set; } = string.Empty;
    public string ClinicId { get; set; } = string.Empty;
    public string AppointmentId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Temperature { get; set; }

    public string VetName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
}

public class CreateMedicalRecordDto
{
    public string PetId { get; set; } = string.Empty;
    public string AppointmentId { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Temperature { get; set; }
}

public class UpdateMedicalRecordDto
{
    public DateTime? Date { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Temperature { get; set; }
}
