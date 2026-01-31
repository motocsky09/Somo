using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Somo.Server.Entities;

namespace Somo.Server.Services
{
    public static class DbSeeder
    {
        public static async Task InitializeAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        }
    }
}
