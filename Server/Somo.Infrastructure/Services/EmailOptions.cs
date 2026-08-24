namespace Somo.Infrastructure.Services;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "no-reply@somo.ro";
    public string FromName { get; set; } = "Somo";
    public bool UseStartTls { get; set; } = true;

    /// <summary>
    /// Unde ajung emailurile când nu există server SMTP configurat.
    /// </summary>
    public string OutboxDirectory { get; set; } = "Outbox";

    /// <summary>
    /// Fără host și user nu avem cu ce trimite, așa că emailurile se scriu pe disc.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Username);
}
