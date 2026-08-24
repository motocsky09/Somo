using Somo.Domain.Entities;

namespace Somo.Application.Interfaces;

/// <summary>
/// Notificările trimise proprietarului pe parcursul unei programări și înainte de
/// rapelul unui vaccin. Nicio metodă nu aruncă: o notificare nelivrată nu trebuie
/// să blocheze acțiunea care a declanșat-o.
/// </summary>
public interface INotificationService
{
    Task AppointmentCreatedAsync(Appointment appointment);
    Task AppointmentConfirmedAsync(Appointment appointment);
    Task AppointmentCancelledAsync(Appointment appointment);
    Task VaccinationReminderAsync(Vaccination vaccination);
    Task VetAccountCreatedAsync(Vet vet, string username, string temporaryPassword);
}
