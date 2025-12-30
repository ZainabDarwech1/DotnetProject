namespace LebAssist.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendWelcomeEmailAsync(string to, string userName);
        Task SendProviderApprovedEmailAsync(string to, string userName);
        Task SendProviderRejectedEmailAsync(string to, string userName, string reason);
        Task SendBookingNotificationEmailAsync(string to, string userName, int bookingId, string status);
        Task SendEmergencyAcceptedEmailAsync(string to, string providerName, string providerPhone);
        Task SendPasswordResetEmailAsync(string to, string resetLink);
    }
}