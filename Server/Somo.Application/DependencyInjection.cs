using Microsoft.Extensions.DependencyInjection;
using Somo.Application.Features.Appointments.Commands;
using Somo.Application.Features.Appointments.Queries;

namespace Somo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateAppointmentCommand>();
        services.AddScoped<GetAvailableSlotsQuery>();
        return services;
    }
}