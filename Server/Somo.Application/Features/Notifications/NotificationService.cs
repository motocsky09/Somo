using Microsoft.Extensions.Logging;
using Somo.Application.Interfaces;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Application.Features.Notifications;

public class NotificationService : INotificationService
{
    private readonly IEmailSender _email;
    private readonly IUserDirectory _users;
    private readonly IPetRepository _pets;
    private readonly IVetRepository _vets;
    private readonly IVeterinaryClinicRepository _clinics;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEmailSender email,
        IUserDirectory users,
        IPetRepository pets,
        IVetRepository vets,
        IVeterinaryClinicRepository clinics,
        ILogger<NotificationService> logger)
    {
        _email = email;
        _users = users;
        _pets = pets;
        _vets = vets;
        _clinics = clinics;
        _logger = logger;
    }

    public Task AppointmentCreatedAsync(Appointment appointment) =>
        SendAppointmentEmailAsync(
            appointment,
            subject: "Programarea ta a fost înregistrată",
            heading: "Am înregistrat programarea",
            introduction: owner =>
                $"Bună, {EmailTemplates.Escape(owner)}! Cererea ta de programare a ajuns la cabinet. " +
                "Vei primi un email de îndată ce cabinetul o confirmă.",
            callout: EmailTemplates.Callout(
                "Programarea este în așteptarea confirmării cabinetului."));

    public Task AppointmentConfirmedAsync(Appointment appointment) =>
        SendAppointmentEmailAsync(
            appointment,
            subject: "Programarea ta a fost confirmată",
            heading: "Programare confirmată",
            introduction: owner =>
                $"Bună, {EmailTemplates.Escape(owner)}! Cabinetul ți-a confirmat programarea. Te așteptăm!",
            callout: EmailTemplates.Callout(
                "Dacă nu mai poți ajunge, anunță cabinetul din timp ca intervalul să fie eliberat.",
                accent: "#2ecc71"));

    public Task AppointmentCancelledAsync(Appointment appointment) =>
        SendAppointmentEmailAsync(
            appointment,
            subject: "Programarea ta a fost anulată",
            heading: "Programare anulată",
            introduction: owner =>
                $"Bună, {EmailTemplates.Escape(owner)}! Programarea de mai jos a fost anulată.",
            callout: EmailTemplates.Callout(
                "Poți face oricând o programare nouă din contul tău Somo.",
                accent: "#e74c3c"));

    public async Task VaccinationReminderAsync(Vaccination vaccination)
    {
        try
        {
            var owner = await _users.GetContactAsync(vaccination.OwnerId);
            if (owner is null || string.IsNullOrWhiteSpace(owner.Email))
            {
                _logger.LogWarning(
                    "Reminder de vaccin neexpediat: proprietarul {OwnerId} nu are email.",
                    vaccination.OwnerId);
                return;
            }

            var pet = await _pets.GetByIdAsync(vaccination.PetId);
            var clinic = await _clinics.GetByIdAsync(vaccination.ClinicId);
            var petName = pet?.Name ?? "animalul tău";
            var daysLeft = (vaccination.NextDueOn.Date - DateTime.UtcNow.Date).Days;

            var body = EmailTemplates.DetailsTable(
                ("Animal", petName),
                ("Vaccin", vaccination.VaccineName),
                ("Administrat la", EmailTemplates.FormatDate(vaccination.AdministeredOn)),
                ("Rapel programat pentru", EmailTemplates.FormatDate(vaccination.NextDueOn)),
                ("Cabinet", clinic?.Name),
                ("Telefon cabinet", clinic?.Phone));

            var whenText = daysLeft switch
            {
                <= 0 => "este scadent astăzi",
                1 => "este scadent mâine",
                _ => $"este scadent în {daysLeft} zile"
            };

            var html = EmailTemplates.Layout(
                heading: $"Rapelul pentru {EmailTemplates.Escape(petName)} se apropie",
                introduction:
                    $"Bună, {EmailTemplates.Escape(owner.FullName)}! Rapelul la " +
                    $"<strong>{EmailTemplates.Escape(vaccination.VaccineName)}</strong> pentru " +
                    $"{EmailTemplates.Escape(petName)} {whenText}.",
                bodyHtml: body + EmailTemplates.Callout(
                    "Sună cabinetul sau intră în contul tău Somo ca să faci programarea pentru rapel."),
                footerNote: "Îți trimitem un singur mesaj pentru fiecare rapel.");

            await _email.SendAsync(new EmailMessage(
                owner.Email,
                $"Rapel {vaccination.VaccineName} pentru {petName}",
                html,
                owner.FullName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Reminderul pentru vaccinul {VaccinationId} nu a putut fi trimis.", vaccination.Id);
        }
    }

    public async Task VetAccountCreatedAsync(Vet vet, string username, string temporaryPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(vet.Email))
                return;

            var body = EmailTemplates.DetailsTable(
                ("Utilizator", username),
                ("Parolă temporară", temporaryPassword));

            var html = EmailTemplates.Layout(
                heading: "Contul tău de medic a fost creat",
                introduction:
                    $"Bună, Dr. {EmailTemplates.Escape($"{vet.FirstName} {vet.LastName}".Trim())}! " +
                    "Cabinetul ți-a creat un cont Somo. Cu datele de mai jos îți vezi propria agendă, " +
                    "completezi fișele medicale și înregistrezi vaccinurile.",
                bodyHtml: body + EmailTemplates.Callout(
                    "Schimbă parola temporară din secțiunea „Contul meu” imediat după prima autentificare.",
                    accent: "#e74c3c"));

            await _email.SendAsync(new EmailMessage(
                vet.Email,
                "Datele tale de acces în Somo",
                html,
                $"{vet.FirstName} {vet.LastName}".Trim()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Emailul cu date de acces pentru medicul {VetId} nu a putut fi trimis.", vet.Id);
        }
    }

    private async Task SendAppointmentEmailAsync(
        Appointment appointment,
        string subject,
        string heading,
        Func<string, string> introduction,
        string callout)
    {
        try
        {
            var owner = await _users.GetContactAsync(appointment.OwnerId);
            if (owner is null || string.IsNullOrWhiteSpace(owner.Email))
            {
                _logger.LogWarning(
                    "Notificare de programare neexpediată: proprietarul {OwnerId} nu are email.",
                    appointment.OwnerId);
                return;
            }

            var pet = await _pets.GetByIdAsync(appointment.PetId);
            var vet = await _vets.GetByIdAsync(appointment.VetId);
            var clinic = await _clinics.GetByIdAsync(appointment.ClinicId);

            var body = EmailTemplates.DetailsTable(
                ("Animal", pet?.Name),
                ("Data și ora", EmailTemplates.FormatDateTime(appointment.DateTime)),
                ("Medic", vet is null ? null : $"Dr. {vet.FirstName} {vet.LastName}".Trim()),
                ("Cabinet", clinic?.Name),
                ("Adresă", clinic?.Address),
                ("Telefon cabinet", clinic?.Phone),
                ("Motiv", appointment.Reason));

            var html = EmailTemplates.Layout(
                heading,
                introduction(owner.FullName),
                body + callout);

            await _email.SendAsync(new EmailMessage(owner.Email, subject, html, owner.FullName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Notificarea pentru programarea {AppointmentId} nu a putut fi trimisă.", appointment.Id);
        }
    }
}
