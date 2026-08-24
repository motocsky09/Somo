using Microsoft.AspNetCore.Identity;
using Somo.API.Entities;
using Somo.Application.Interfaces;

namespace Somo.API.Services;

public class IdentityUserDirectory : IUserDirectory
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserDirectory(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<UserContact?> GetContactAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return null;

        return new UserContact(
            user.Id.ToString(),
            user.UserName ?? string.Empty,
            user.FirstName ?? string.Empty,
            user.LastName ?? string.Empty,
            user.Email ?? string.Empty,
            user.PhoneNumber ?? string.Empty,
            user.ProfilePhotoUrl);
    }
}
