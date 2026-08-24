using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Somo.Application.Interfaces;

namespace Somo.Infrastructure.Services;

/// <summary>
/// Trimite prin SMTP când există configurație; altfel scrie mesajul în Outbox,
/// ca aplicația să fie utilizabilă imediat după clonare, fără credențiale.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message.To))
        {
            _logger.LogWarning("Email neexpediat ({Subject}): destinatarul lipsește.", message.Subject);
            return;
        }

        var mime = BuildMessage(message);

        if (!_options.IsConfigured)
        {
            await WriteToOutboxAsync(mime, message, cancellationToken);
            return;
        }

        using var client = new SmtpClient();
        var secureOption = _options.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.SslOnConnect;

        await client.ConnectAsync(_options.Host, _options.Port, secureOption, cancellationToken);
        await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email trimis către {To}: {Subject}", message.To, message.Subject);
    }

    private MimeMessage BuildMessage(EmailMessage message)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mime.To.Add(new MailboxAddress(message.ToDisplayName ?? string.Empty, message.To));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();
        return mime;
    }

    private async Task WriteToOutboxAsync(
        MimeMessage mime, EmailMessage message, CancellationToken cancellationToken)
    {
        var directory = Path.IsPathRooted(_options.OutboxDirectory)
            ? _options.OutboxDirectory
            : Path.Combine(AppContext.BaseDirectory, _options.OutboxDirectory);

        Directory.CreateDirectory(directory);

        var fileName = $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}.eml";
        var path = Path.Combine(directory, fileName);

        await using var stream = File.Create(path);
        await mime.WriteToAsync(stream, cancellationToken);

        _logger.LogInformation(
            "SMTP neconfigurat. Emailul pentru {To} ({Subject}) a fost salvat în {Path}.",
            message.To, message.Subject, path);
    }
}
