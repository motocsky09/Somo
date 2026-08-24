namespace Somo.Application.Interfaces;

public record EmailMessage(string To, string Subject, string HtmlBody, string? ToDisplayName = null);

/// <summary>
/// Transportul propriu-zis al emailurilor. Șabloanele stau în stratul Application,
/// implementarea (SMTP sau scriere pe disc în dezvoltare) stă în Infrastructure.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
