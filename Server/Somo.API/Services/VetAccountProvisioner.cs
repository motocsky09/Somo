using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Somo.API.Entities;
using Somo.Application.Common;

namespace Somo.API.Services;

public record VetAccountResult(bool Success, string Username, string Password, string UserId, string Error);

/// <summary>
/// Creează contul de autentificare al unui medic. Parola temporară este returnată
/// o singură dată, la creare, ca să poată fi predată medicului.
/// </summary>
public class VetAccountProvisioner
{
    private const string PasswordAlphabet =
        "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public VetAccountProvisioner(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<VetAccountResult> CreateAsync(
        string firstName, string lastName, string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(email))
            return new VetAccountResult(false, "", "", "", "Medicul are nevoie de o adresă de email pentru cont.");

        if (await _userManager.FindByEmailAsync(email) is not null)
            return new VetAccountResult(false, "", "", "", "Există deja un cont cu această adresă de email.");

        var username = await BuildUniqueUsernameAsync(firstName, lastName);
        var password = GeneratePassword();

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
            FirstName = firstName,
            LastName = lastName,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new VetAccountResult(false, "", "", "", "Contul medicului nu a putut fi creat: " + errors);
        }

        if (!await _roleManager.RoleExistsAsync(AppRoles.Vet))
            await _roleManager.CreateAsync(new ApplicationRole { Name = AppRoles.Vet });

        await _userManager.AddToRoleAsync(user, AppRoles.Vet);

        return new VetAccountResult(true, username, password, user.Id.ToString(), string.Empty);
    }

    public async Task DeleteAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
            await _userManager.DeleteAsync(user);
    }

    /// <summary>
    /// „dr.ion.popescu”, cu sufix numeric dacă numele este deja luat.
    /// </summary>
    private async Task<string> BuildUniqueUsernameAsync(string firstName, string lastName)
    {
        var basis = $"dr.{Slug(firstName)}.{Slug(lastName)}".Trim('.');
        if (basis == "dr")
            basis = "dr.medic";

        var candidate = basis;
        var suffix = 1;
        while (await _userManager.FindByNameAsync(candidate) is not null)
            candidate = $"{basis}{++suffix}";

        return candidate;
    }

    private static string Slug(string value)
    {
        var normalized = value.Trim().ToLowerInvariant()
            .Replace("ă", "a").Replace("â", "a").Replace("î", "i")
            .Replace("ș", "s").Replace("ş", "s")
            .Replace("ț", "t").Replace("ţ", "t");

        var chars = normalized.Where(c => char.IsLetterOrDigit(c)).ToArray();
        return new string(chars);
    }

    private static string GeneratePassword()
    {
        var chars = new char[12];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = PasswordAlphabet[RandomNumberGenerator.GetInt32(PasswordAlphabet.Length)];

        return new string(chars);
    }
}
