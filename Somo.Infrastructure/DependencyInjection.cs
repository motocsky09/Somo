using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Somo.Domain.Interfaces;
using Somo.Infrastructure.Repositories;

namespace Somo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mongoConnectionString = configuration.GetConnectionString("MongoDb");
        var mongoDatabaseName = configuration["MongoDbSettings:DatabaseName"];

        services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoDatabaseName);
        });

        services.AddScoped<IMedicsRepository, MedicsRepository>();
        services.AddScoped<IMedServicesRepository, MedServicesRepository>();
        services.AddScoped<ISchedulingRepository, SchedulingRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));

        return services;
    }
}