using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Somo.API.Entities;

namespace Somo.API.Services
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
