using AISAM.Common.Config;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AISAM.Services.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly string _frontendBaseUrl;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            IOptions<FrontendSettings> frontendSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _frontendBaseUrl = frontendSettings.Value.BaseUrl;
        }

        public async Task SendEmailVerificationAsync(string email, string userName, string verificationToken)
        {
            var verificationLink = $"{_frontendBaseUrl}/auth/verify-email?token={verificationToken}";
            var subject = "Verify your AISAM email";
            var htmlBody = BuildEmailTemplate(
                "Verify your email",
                $"Hello {userName},",
                "Thank you for registering an AISAM account. Please verify your email address using the button below.",
                "Verify email",
                verificationLink,
                "This link expires after 7 days.");
            var plainTextBody = $"Hello {userName},\n\nPlease verify your AISAM email address:\n{verificationLink}\n\nThis link expires after 7 days.";

            await SendEmailAsync(email, subject, htmlBody, plainTextBody);
            _logger.LogInformation("Email verification sent to {Email}", email);
        }

        public async Task SendPasswordResetAsync(string email, string userName, string resetToken)
        {
            var resetLink = $"{_frontendBaseUrl}/reset-password?token={resetToken}";
            var subject = "Reset your AISAM password";
            var htmlBody = BuildEmailTemplate(
                "Reset your password",
                $"Hello {userName},",
                "We received a request to reset your AISAM password. Use the button below to choose a new password.",
                "Reset password",
                resetLink,
                "This link expires after 1 hour. If you did not request this change, ignore this email.");
            var plainTextBody = $"Hello {userName},\n\nReset your AISAM password:\n{resetLink}\n\nThis link expires after 1 hour. If you did not request this change, ignore this email.";

            await SendEmailAsync(email, subject, htmlBody, plainTextBody);
            _logger.LogInformation("Password reset email sent to {Email}", email);
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            var subject = "Welcome to AISAM";
            var htmlBody = BuildSimpleTemplate(
                "Welcome to AISAM",
                $"Hello {userName},",
                "Welcome to AISAM. You can now manage profiles, brands, products, content, and campaigns from one workspace.");
            var plainTextBody = $"Hello {userName},\n\nWelcome to AISAM. You can now manage profiles, brands, products, content, and campaigns from one workspace.";

            await SendEmailAsync(email, subject, htmlBody, plainTextBody);
            _logger.LogInformation("Welcome email sent to {Email}", email);
        }

        public async Task SendTeamInvitationAsync(string email, string teamName, string inviterName, string invitationLink)
        {
            var subject = $"Invitation to join {teamName}";
            var htmlBody = BuildEmailTemplate(
                "Team invitation",
                $"Hello,",
                $"{inviterName} invited you to join the team '{teamName}' on AISAM.",
                "Accept invitation",
                invitationLink,
                "Only accept this invitation if you recognize the sender.");
            var plainTextBody = $"{inviterName} invited you to join the team '{teamName}' on AISAM.\n\nOpen this link to accept:\n{invitationLink}";

            await SendEmailAsync(email, subject, htmlBody, plainTextBody);
            _logger.LogInformation("Team invitation sent to {Email} for team {TeamName}", email, teamName);
        }

        public async Task SendNotificationEmailAsync(string email, string subject, string message)
        {
            var htmlBody = BuildSimpleTemplate("AISAM notification", "Hello,", message);
            var plainTextBody = message;

            await SendEmailAsync(email, subject, htmlBody, plainTextBody);
            _logger.LogInformation("Notification email sent to {Email}", email);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_emailSettings.SmtpHost) ||
                    string.IsNullOrWhiteSpace(_emailSettings.SmtpUsername))
                {
                    _logger.LogWarning("Email settings not configured. Email not sent to {Email}", toEmail);
                    return false;
                }

                _logger.LogInformation(
                    "Sending email to {Email} with subject '{Subject}' via {SmtpHost}:{Port}",
                    toEmail,
                    subject,
                    _emailSettings.SmtpHost,
                    _emailSettings.SmtpPort);

                using var smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    SubjectEncoding = Encoding.UTF8,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(toEmail);

                if (!string.IsNullOrWhiteSpace(plainTextBody))
                {
                    var plainView = AlternateView.CreateAlternateViewFromString(plainTextBody, Encoding.UTF8, "text/plain");
                    mailMessage.AlternateViews.Add(plainView);
                }

                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
                mailMessage.AlternateViews.Add(htmlView);

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(
                    ex,
                    "SMTP error sending email to {Email}: StatusCode={StatusCode}, Message={Message}",
                    toEmail,
                    ex.StatusCode,
                    ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}: {Message}", toEmail, ex.Message);
                return false;
            }
        }

        private static string BuildEmailTemplate(
            string title,
            string greeting,
            string body,
            string buttonText,
            string link,
            string note)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #3b82f6; color: white; padding: 24px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8fafc; padding: 24px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 24px; background: #2563eb; color: white; text-decoration: none; border-radius: 4px; margin: 16px 0; }}
        .link {{ word-break: break-all; background: white; padding: 10px; border-radius: 4px; }}
        .note {{ color: #555; font-size: 14px; }}
        .footer {{ text-align: center; margin-top: 24px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>AISAM</h1>
            <p>{WebUtility.HtmlEncode(title)}</p>
        </div>
        <div class=""content"">
            <h2>{WebUtility.HtmlEncode(greeting)}</h2>
            <p>{WebUtility.HtmlEncode(body)}</p>
            <p style=""text-align: center;""><a href=""{WebUtility.HtmlEncode(link)}"" class=""button"">{WebUtility.HtmlEncode(buttonText)}</a></p>
            <p>Or copy and paste this link into your browser:</p>
            <p class=""link"">{WebUtility.HtmlEncode(link)}</p>
            <p class=""note"">{WebUtility.HtmlEncode(note)}</p>
        </div>
        <div class=""footer"">
            <p>This email was sent automatically. Please do not reply.</p>
            <p>&copy; 2026 AISAM. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string BuildSimpleTemplate(string title, string greeting, string body)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <h1>{WebUtility.HtmlEncode(title)}</h1>
        <h2>{WebUtility.HtmlEncode(greeting)}</h2>
        <p>{WebUtility.HtmlEncode(body)}</p>
        <p style=""color: #666; font-size: 12px;"">This email was sent automatically. Please do not reply.</p>
    </div>
</body>
</html>";
        }
    }
}
