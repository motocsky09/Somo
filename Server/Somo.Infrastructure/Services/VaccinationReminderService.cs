using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Somo.Application.Interfaces;
using Somo.Domain.Interfaces;

namespace Somo.Infrastructure.Services;

/// <summary>
/// Trimite zilnic reminderele de rapel. Fiecare vaccin primește un singur mesaj:
/// data expedierii se scrie pe înregistrare înainte de a trece la următoarea.
/// </summary>
public class VaccinationReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly VaccinationReminderOptions _options;
    private readonly ILogger<VaccinationReminderService> _logger;

    public VaccinationReminderService(
        IServiceScopeFactory scopeFactory,
        IOptions<VaccinationReminderOptions> options,
        ILogger<VaccinationReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Reminderele de vaccinare sunt dezactivate din configurație.");
            return;
        }

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(_options.StartupDelaySeconds), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        var interval = TimeSpan.FromHours(Math.Max(1, _options.RunIntervalHours));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendDueRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rularea reminderelor de vaccinare a eșuat.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public async Task<int> SendDueRemindersAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var vaccinations = scope.ServiceProvider.GetRequiredService<IVaccinationRepository>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var today = DateTime.UtcNow.Date;
        var from = today.AddDays(-Math.Abs(_options.CatchUpDays));
        var to = today.AddDays(Math.Abs(_options.LeadDays));

        var due = (await vaccinations.GetDueWithoutReminderAsync(from, to)).ToList();
        if (due.Count == 0)
            return 0;

        var sent = 0;
        foreach (var vaccination in due)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await notifications.VaccinationReminderAsync(vaccination);

            vaccination.ReminderSentAtUtc = DateTime.UtcNow;
            await vaccinations.UpdateAsync(vaccination);
            sent++;
        }

        _logger.LogInformation("Au fost trimise {Count} remindere de rapel.", sent);
        return sent;
    }
}
