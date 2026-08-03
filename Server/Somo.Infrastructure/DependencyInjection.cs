using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Somo.Domain.Interfaces;
using Somo.Infrastructure.Repositories;
using Somo.Application.Interfaces;
using Somo.Infrastructure.Services;

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

        services.AddScoped<IVetRepository, VetRepository>();
        services.AddScoped<IVeterinaryClinicRepository, VeterinaryClinicRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        services.AddSingleton<IPlacesCacheService, FilePlacesCacheService>();
        services.AddScoped<IClinicRegistrationService, ClinicRegistrationService>();
        services.AddHttpClient<IGooglePlacesService, GooglePlacesService>();

        return services;
    }
}