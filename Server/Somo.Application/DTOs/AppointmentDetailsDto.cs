namespace Somo.Application.DTOs;

public class AppointmentPetDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int Age { get; set; }
    public double Weight { get; set; }
    public string? PhotoUrl { get; set; }
}

public class AppointmentVetDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string FullName => $"Dr. {FirstName} {LastName}".Trim();
}

public class AppointmentOwnerDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }

    public string FullName =>
        string.Join(' ', new[] { FirstName, LastName }.Where(n => !string.IsNullOrWhiteSpace(n)));
}

public class AppointmentDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string PetId { get; set; } = string.Empty;
    public string VetId { get; set; } = string.Empty;
    public string ClinicId { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Status { get; set; }

    public AppointmentPetDto? Pet { get; set; }
    public AppointmentVetDto? Vet { get; set; }
    public AppointmentOwnerDto? Owner { get; set; }
}
