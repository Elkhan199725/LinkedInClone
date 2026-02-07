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
[ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger by default (still callable)
public sealed class DevController : ControllerBase
{
    private readonly IEmailSender _emailSender;
    private readonly SmtpSettings _smtp;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DevController> _logger;

    public DevController(
        IEmailSender emailSender,
        IOptions<SmtpSettings> smtp,
        IWebHostEnvironment env,
        ILogger<DevController> logger)
    {
        _emailSender = emailSender;
        _smtp = smtp.Value;
        _env = env;
        _logger = logger;
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
            isValid = _smtp.IsValid,
            host = _smtp.Host,
            port = _smtp.Port,
            enableSsl = _smtp.EnableSsl,
            username = _smtp.Username,
            fromEmail = _smtp.FromEmail,
            fromName = _smtp.FromName,
            passwordSet = !string.IsNullOrWhiteSpace(_smtp.Password),
            passwordLength = _smtp.Password?.Length ?? 0 // you can remove this later if you want
        });
    }

    [HttpGet("test-email-simple")]
    public async Task<IActionResult> TestEmailSimple(CancellationToken ct)
    {
        if (!IsDev()) return DevOnly();

        var toEmail = _smtp.Username; // send to yourself for a deterministic test
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

            return StatusCode(500, new
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
            return BadRequest(new { success = false, error = "toEmail is required." });

        _logger.LogInformation("Dev test email sending to {ToEmail}", request.ToEmail);

        try
        {
            await _emailSender.SendEmailAsync(
                request.ToEmail.Trim(),
                "LinkedInClone Test Email",
                $"<p>Test email sent at {DateTime.UtcNow:O}</p>",
                ct);

            return Ok(new { success = true, message = "Email sent successfully.", toEmail = request.ToEmail });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dev test email failed");

            return StatusCode(500, new
            {
                success = false,
                errorType = ex.GetType().Name,
                message = ex.Message,
                innerMessage = ex.InnerException?.Message
            });
        }
    }

    /// <summary>
    /// Low-level SMTP authentication test for debugging only.
    /// This bypasses appsettings binding issues by using explicit credentials.
    /// Provide values via query string to avoid hardcoding secrets in source code.
    ///
    /// Example:
    /// GET /api/dev/smtp-auth-test?email=you@gmail.com&appPassword=xxxx%20xxxx%20xxxx%20xxxx
    /// </summary>
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

        // IMPORTANT: appPassword may contain spaces (as shown by Google). Do not trim internal spaces.
        appPassword = appPassword.Trim();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("LinkedInClone Manual Test", email));
        message.To.Add(new MailboxAddress("Test Recipient", email));
        message.Subject = "SMTP Auth Test - LinkedInClone";
        message.Body = new TextPart("plain") { Text = "Hello. This is a low-level SMTP auth test." };

        using var client = new SmtpClient();

        try
        {
            client.Timeout = 20_000;
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

            return StatusCode(500, new
            {
                success = false,
                errorType = ex.GetType().Name,
                message = ex.Message,
                innerMessage = ex.InnerException?.Message
            });
        }
    }
}
