using LebAssist.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace LebAssist.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"] ?? "";
                var smtpPass = _configuration["Email:SmtpPassword"] ?? "";
                var fromEmail = _configuration["Email:FromEmail"] ?? smtpUser;
                var fromName = _configuration["Email:FromName"] ?? "LebAssist";

                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    _logger.LogWarning("Email not configured. Skipping email to {To}", to);
                    return;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPass)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", to);
            }
        }

        public async Task SendWelcomeEmailAsync(string to, string userName)
        {
            var subject = "Welcome to LebAssist!";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Welcome to LebAssist, {userName}!</h2>
                    <p>Thank you for joining our platform.</p>
                    <p>You can now:</p>
                    <ul>
                        <li>Browse and book services</li>
                        <li>Request emergency assistance</li>
                        <li>Apply to become a service provider</li>
                    </ul>
                    <p>Best regards,<br/>The LebAssist Team</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendProviderApprovedEmailAsync(string to, string userName)
        {
            var subject = "Your Provider Application is Approved! 🎉";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Congratulations, {userName}!</h2>
                    <p>Your provider application has been <strong>approved</strong>.</p>
                    <p>You can now start accepting booking requests and emergency calls.</p>
                    <p>Best regards,<br/>The LebAssist Team</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendProviderRejectedEmailAsync(string to, string userName, string reason)
        {
            var subject = "Provider Application Status";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Hello, {userName}</h2>
                    <p>Unfortunately, your provider application was not approved at this time.</p>
                    <p><strong>Reason:</strong> {reason}</p>
                    <p>You may reapply after addressing the above concerns.</p>
                    <p>Best regards,<br/>The LebAssist Team</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendBookingNotificationEmailAsync(string to, string userName, int bookingId, string status)
        {
            var subject = $"Booking #{bookingId} - {status}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Booking Update</h2>
                    <p>Hello {userName},</p>
                    <p>Your booking #{bookingId} status has been updated to: <strong>{status}</strong></p>
                    <p>Log in to LebAssist for more details.</p>
                    <p>Best regards,<br/>The LebAssist Team</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendEmergencyAcceptedEmailAsync(string to, string providerName, string providerPhone)
        {
            var subject = "🚨 Emergency Accepted - Help is on the way!";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Your Emergency Has Been Accepted!</h2>
                    <p>A provider is on their way to help you.</p>
                    <p><strong>Provider:</strong> {providerName}</p>
                    <p><strong>Phone:</strong> <a href='tel:{providerPhone}'>{providerPhone}</a></p>
                    <p>Please keep your phone nearby.</p>
                    <p>Best regards,<br/>The LebAssist Team</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var subject = "Reset Your Password";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Password Reset Request</h2>
                    <p>Click the link below to reset your password:</p>
                    <p><a href='{resetLink}'>Reset Password</a></p>
                    <p>If you did not request this, please ignore this email.</p>
                    <p>Best regards,<br/>The LebAssist Team</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }
    }
}