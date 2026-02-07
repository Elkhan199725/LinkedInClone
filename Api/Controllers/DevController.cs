using Application.Common.Interfaces;
using Infrastructure.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Api.Controllers;

[ApiController]
[Route("api/dev")]
public sealed class DevController : ControllerBase
{
    private readonly IEmailSender _emailSender;
    private readonly SmtpSettings _smtp;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DevController> _logger;
    private readonly IConfiguration _configuration;

    public DevController(
        IEmailSender emailSender,
        IOptions<SmtpSettings> smtp,
        IWebHostEnvironment env,
        ILogger<DevController> logger,
        IConfiguration configuration)
    {
        _emailSender = emailSender;
        _smtp = smtp.Value;
        _env = env;
        _logger = logger;
        _configuration = configuration;
    }

    private IActionResult DevOnly()
        => NotFound(new { success = false, error = "This endpoint is available only in Development." });

    private bool IsDev() => _env.IsDevelopment();

    [HttpGet("smtp-status")]
    public IActionResult SmtpStatus()
    {
        if (!IsDev()) return DevOnly();

        return Ok(new
        {
            environment = _env.EnvironmentName,
            host = _smtp.Host,
            port = _smtp.Port,
            enableSsl = _smtp.EnableSsl,
            username = _smtp.Username,
            fromEmail = _smtp.FromEmail,
            fromName = _smtp.FromName,
            passwordSet = !string.IsNullOrWhiteSpace(_smtp.Password),
            passwordLength = _smtp.Password?.Length ?? 0,
            isValid = _smtp.IsValid
        });
    }

    [HttpGet("config-status")]
    public IActionResult ConfigStatus()
    {
        if (!IsDev()) return DevOnly();

        var connStr = _configuration.GetConnectionString("DefaultConnection") ?? "";
        var hasDbPassword = connStr.Contains("Password=") && 
                           !connStr.Contains("Password=__") && 
                           !connStr.Contains("Password=;") &&
                           !connStr.Contains("Password=\"\"");

        return Ok(new
        {
            environment = _env.EnvironmentName,
            userSecretsEnabled = _env.IsDevelopment(),
            database = new
            {
                connectionStringSet = !string.IsNullOrWhiteSpace(connStr),
                passwordSet = hasDbPassword
            },
            smtp = new
            {
                host = _smtp.Host,
                port = _smtp.Port,
                username = _smtp.Username,
                passwordSet = !string.IsNullOrWhiteSpace(_smtp.Password),
                passwordLength = _smtp.Password?.Length ?? 0,
                isValid = _smtp.IsValid
            },
            jwt = new
            {
                keySet = !string.IsNullOrWhiteSpace(_configuration["Jwt:Key"]),
                keyLength = _configuration["Jwt:Key"]?.Length ?? 0
            }
        });
    }

    [HttpGet("test-email-simple")]
    public async Task<IActionResult> TestEmailSimple(CancellationToken ct)
    {
        if (!IsDev()) return DevOnly();

        if (!_smtp.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                error = "SMTP not configured properly",
                host = _smtp.Host,
                username = _smtp.Username,
                passwordSet = !string.IsNullOrWhiteSpace(_smtp.Password)
            });
        }

        var toEmail = _smtp.Username;
        var subject = "SMTP Test - LinkedInClone";
        var body = "<h2>Hello</h2><p>This is a dev test email from LinkedInClone.</p>";

        _logger.LogInformation("Dev test email (simple) sending to {ToEmail}", toEmail);

        try
        {
            await _emailSender.SendEmailAsync(toEmail, subject, body, ct);

            return Ok(new
            {
                success = true,
                message = "Email sent successfully.",
                toEmail
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dev test email (simple) failed");

            return Ok(new
            {
                success = false,
                errorType = ex.GetType().Name,
                message = ex.Message,
                innerMessage = ex.InnerException?.Message
            });
        }
    }

    public sealed record TestEmailRequest(string ToEmail);

    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request, CancellationToken ct)
    {
        if (!IsDev()) return DevOnly();

        if (string.IsNullOrWhiteSpace(request?.ToEmail))
            return BadRequest(new { success = false, error = "toEmail required" });

        if (!_smtp.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                error = "SMTP not configured properly"
            });
        }

        try
        {
            await _emailSender.SendEmailAsync(
                request.ToEmail,
                "LinkedInClone Test",
                $"<p>Test email sent at {DateTime.UtcNow:O}</p>",
                ct);

            return Ok(new { success = true, message = $"Sent to {request.ToEmail}" });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                success = false,
                errorType = ex.GetType().Name,
                message = ex.Message,
                innerMessage = ex.InnerException?.Message
            });
        }
    }

    [HttpGet("smtp-auth-test")]
    public async Task<IActionResult> SmtpAuthTest(
        [FromQuery] string email,
        [FromQuery] string appPassword,
        CancellationToken ct)
    {
        if (!IsDev()) return DevOnly();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(appPassword))
            return BadRequest(new { success = false, error = "email and appPassword query parameters are required." });

        email = email.Trim();
        appPassword = appPassword.Trim();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("LinkedInClone Manual Test", email));
        message.To.Add(new MailboxAddress("Test Recipient", email));
        message.Subject = "SMTP Auth Test - LinkedInClone";
        message.Body = new TextPart("plain") { Text = "Hello. This is a low-level SMTP auth test." };

        using var client = new SmtpClient();

        try
        {
            client.Timeout = 20000;
            client.AuthenticationMechanisms.Remove("XOAUTH2");

            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(email, appPassword, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            return Ok(new
            {
                success = true,
                message = "SMTP authentication and send succeeded.",
                email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP auth test failed for {Email}", email);

            return Ok(new
            {
                success = false,
                errorType = ex.GetType().Name,
                message = ex.Message,
                innerMessage = ex.InnerException?.Message
            });
        }
    }
}
