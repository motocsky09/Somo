using System.ComponentModel.DataAnnotations;
using Somo.Application.DTOs;

namespace Server.Models;

public class RegisterModel
{
    [Required(ErrorMessage = "User Name is required")]
    public string? Username { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }

    public string? Role { get; set; }

    public RegisterClinicDto? Clinic { get; set; }
}
