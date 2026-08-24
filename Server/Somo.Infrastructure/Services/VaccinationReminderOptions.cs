namespace Somo.Infrastructure.Services;

public class VaccinationReminderOptions
{
    public const string SectionName = "VaccinationReminders";

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Cu câte zile înainte de rapel pleacă reminderul.
    /// </summary>
    public int LeadDays { get; set; } = 7;

    /// <summary>
    /// Cât de departe în trecut recuperăm rapelurile pentru care nu a plecat nimic,
    /// ca o oprire a aplicației să nu însemne remindere pierdute definitiv.
    /// </summary>
    public int CatchUpDays { get; set; } = 30;

    public int RunIntervalHours { get; set; } = 24;

    /// <summary>
    /// Răgazul de la pornirea aplicației până la prima verificare.
    /// </summary>
    public int StartupDelaySeconds { get; set; } = 30;
}
