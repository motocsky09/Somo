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

            // Ensure unique index on 'CNP' for Clients collection
            var clientsCollection = database.GetCollection<Client>("Clients");
            var clientIndexKeys = Builders<Client>.IndexKeys.Ascending(c => c.CNP);
            var clientIndexModel = new CreateIndexModel<Client>(clientIndexKeys, new CreateIndexOptions { Unique = true });
            await clientsCollection.Indexes.CreateOneAsync(clientIndexModel);

            // Ensure unique index on 'Email' for UserAccounts collection
            var usersCollection = database.GetCollection<UserAccount>("UserAccounts");
            var userIndexKeys = Builders<UserAccount>.IndexKeys.Ascending(u => u.Email);
            var userIndexModel = new CreateIndexModel<UserAccount>(userIndexKeys, new CreateIndexOptions { Unique = true });
            await usersCollection.Indexes.CreateOneAsync(userIndexModel);

            // Seed default Super Admin user if the UserAccounts collection is empty
            if (!await usersCollection.Find(_ => true).AnyAsync())
            {
                var superAdmin = new UserAccount
                {
                    Username = "superadmin",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperSecurePassword123"),
                    IsActive = true
                };
                await usersCollection.InsertOneAsync(superAdmin);
            }
        }
    }
}
