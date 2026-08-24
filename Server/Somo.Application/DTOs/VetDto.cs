namespace Somo.Application.DTOs;

public class CreateVetDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public List<string> ClinicIds { get; set; } = new();
}

/// <summary>
/// Emailul e opțional: dacă lipsește, se folosește cel de pe fișa medicului.
/// </summary>
public class CreateVetAccountDto
{
    public string Email { get; set; } = string.Empty;
}

public class VetDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public List<string> ClinicIds { get; set; } = new();
    public bool HasAccount { get; set; }
}

/// <summary>
/// Răspunsul la crearea unui medic. Parola temporară se întoarce o singură dată,
/// la creare, ca să poată fi comunicată medicului de către cabinet.
/// </summary>
public class VetAccountDto
{
    public VetDto Vet { get; set; } = new();
    public string Username { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
    public bool CredentialsEmailed { get; set; }
}

/// <summary>
/// Fișa medicului autentificat, folosită de interfața proprie.
/// </summary>
public class VetProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public List<VetClinicDto> Clinics { get; set; } = new();

    public string FullName => $"Dr. {FirstName} {LastName}".Trim();
}

public class VetClinicDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
