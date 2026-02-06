using System.Net.Mail;
using Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpSettings> options, ILogger<SmtpEmailSender> logger)
    {
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateSettings(_settings);

        _logger.LogInformation(
            "SMTP settings loaded: Host={Host}, Port={Port}, EnableSsl={EnableSsl}, Username={Username}, FromEmail={FromEmail}, FromName={FromName}, PasswordSet={PasswordSet}",
            _settings.Host,
            _settings.Port,
            _settings.EnableSsl,
            _settings.Username,
            _settings.FromEmail,
            _settings.FromName ?? "(null)",
            !string.IsNullOrWhiteSpace(_settings.Password));
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email is required.", nameof(toEmail));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required.", nameof(subject));
        if (body is null)
            throw new ArgumentNullException(nameof(body));

        // Validate recipient format early
        _ = new MailAddress(toEmail);

        var message = BuildMessage(toEmail, subject, body);

        using var client = new SmtpClient();

        // Gmail username/password auth works best with XOAUTH2 removed
        client.AuthenticationMechanisms.Remove("XOAUTH2");
        client.Timeout = 20_000;

        _logger.LogInformation(
            "SMTP sending: Host={Host}, Port={Port}, Security=StartTls, Username={Username}, To={To}",
            _settings.Host,
            _settings.Port,
            _settings.Username,
            toEmail);

        try
        {
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);

            _logger.LogInformation(
                "SMTP connected: IsSecure={IsSecure}, IsConnected={IsConnected}, Capabilities={Capabilities}",
                client.IsSecure,
                client.IsConnected,
                client.Capabilities);

            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

            _logger.LogInformation("SMTP authenticated successfully as {Username}", _settings.Username);

            await client.SendAsync(message, cancellationToken);

            _logger.LogInformation("SMTP send SUCCESS to {To}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SMTP send FAILED: Type={Type}, Message={Message}, Inner={Inner}",
                ex.GetType().Name,
                ex.Message,
                ex.InnerException?.Message);

            throw;
        }
        finally
        {
            // Ensure we disconnect cleanly
            if (client.IsConnected)
            {
                try
                {
                    await client.DisconnectAsync(true, cancellationToken);
                }
                catch (Exception disconnectEx)
                {
                    _logger.LogWarning(disconnectEx, "SMTP disconnect failed");
                }
            }
        }
    }

    private MimeMessage BuildMessage(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();

        var fromName = string.IsNullOrWhiteSpace(_settings.FromName) ? "LinkedInClone" : _settings.FromName;
        message.From.Add(new MailboxAddress(fromName, _settings.FromEmail));

        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody
        }.ToMessageBody();

        return message;
    }

    private static void ValidateSettings(SmtpSettings s)
    {
        if (string.IsNullOrWhiteSpace(s.Host))
            throw new InvalidOperationException("SMTP Host is missing.");
        if (s.Port <= 0)
            throw new InvalidOperationException("SMTP Port is invalid.");
        if (string.IsNullOrWhiteSpace(s.Username))
            throw new InvalidOperationException("SMTP Username is missing.");
        if (string.IsNullOrWhiteSpace(s.FromEmail))
            throw new InvalidOperationException("SMTP FromEmail is missing.");
        if (string.IsNullOrWhiteSpace(s.Password))
            throw new InvalidOperationException("SMTP Password is missing.");

        // Validate syntax (throws on invalid email)
        _ = new MailAddress(s.Username);
        _ = new MailAddress(s.FromEmail);

        // Gmail: FromEmail should match the authenticated account
        if (!string.Equals(s.Username, s.FromEmail, StringComparison.Ordinal))
            throw new InvalidOperationException("SMTP Username and FromEmail must be identical for Gmail.");
    }
}
