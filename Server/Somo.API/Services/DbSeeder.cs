using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Somo.API.Entities;
using Somo.Application.Common;
using Somo.Domain.Interfaces;

namespace Somo.API.Services
{
    public static class DbSeeder
    {
        public static async Task InitializeAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

            await SeedRolesAsync(services);
            await SeedAdminAsync(services, logger);
            await ApproveLegacyClinicsAsync(services, logger);
        }

        private static async Task SeedRolesAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            foreach (var role in AppRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }

        private static async Task SeedAdminAsync(IServiceProvider services, ILogger logger)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var username = configuration["SomoAdmin:Username"];
            var email = configuration["SomoAdmin:Email"];
            var password = configuration["SomoAdmin:Password"];

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Contul de administrator Somo nu a fost creat: lipsesc setările din secțiunea SomoAdmin.");
                return;
            }

            var existing = await userManager.FindByNameAsync(username);
            if (existing is not null)
            {
                if (!await userManager.IsInRoleAsync(existing, AppRoles.SomoAdmin))
                    await userManager.AddToRoleAsync(existing, AppRoles.SomoAdmin);
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = username,
                Email = email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Contul de administrator Somo nu a putut fi creat: {Errors}", errors);
                return;
            }

            await userManager.AddToRoleAsync(admin, AppRoles.SomoAdmin);
            logger.LogInformation("Cont de administrator Somo creat pentru {Username}.", username);
        }

        private static async Task ApproveLegacyClinicsAsync(IServiceProvider services, ILogger logger)
        {
            var repo = services.GetRequiredService<IVeterinaryClinicRepository>();
            var migrated = await repo.ApproveLegacyClinicsAsync();

            if (migrated > 0)
                logger.LogInformation("{Count} cabinete existente au fost marcate ca aprobate.", migrated);
        }
    }
}
