using Microsoft.AspNetCore.Identity.UI.Services;

namespace HelpDesk.Api.Infrastructure.Security;

/// <summary>Development mail sender: writes the message (with its reset/confirm link) to the log instead of sending it.</summary>
public class DevEmailSender(ILogger<DevEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        logger.LogInformation("[DEV MAIL] To: {Email} | Subject: {Subject}\n{Body}", email, subject, htmlMessage);
        return Task.CompletedTask;
    }
}
