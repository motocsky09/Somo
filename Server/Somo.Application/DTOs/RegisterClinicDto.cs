namespace Somo.Application.DTOs;

public class RegisterClinicDto
{
    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string StreetNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public List<string> VetNames { get; set; } = new();
    public List<ClinicPriceDto> Prices { get; set; } = new();
}

public class ClinicPriceDto
{
    public string Service { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
