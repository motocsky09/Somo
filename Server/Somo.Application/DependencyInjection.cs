using Microsoft.Extensions.DependencyInjection;
using Somo.Application.Features.Appointments.Commands;
using Somo.Application.Features.Appointments.Queries;
using Somo.Application.Features.Medical;
using Somo.Application.Features.Notifications;
using Somo.Application.Features.Vaccinations;
using Somo.Application.Interfaces;

namespace Somo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateAppointmentCommand>();
        services.AddScoped<GetAvailableSlotsQuery>();
        services.AddScoped<AppointmentDetailsMapper>();
        services.AddScoped<PetChartAccessQuery>();
        services.AddScoped<MedicalRecordMapper>();
        services.AddScoped<VaccinationMapper>();
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}
