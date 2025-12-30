using Domain.Enums;
using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface INotificationService
    {
        Task<int> CreateNotificationAsync(string userId, NotificationType type, string title, string message, int? referenceId = null, DateTime? expiryDate = null);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, int take = 20);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
        Task DeleteNotificationAsync(int notificationId);
        Task DeleteOldNotificationsAsync();

        // Specific notification creators
        Task NotifyBookingCreatedAsync(string providerUserId, int bookingId, string clientName);
        Task NotifyBookingStatusChangedAsync(string clientUserId, int bookingId, string status);
        Task NotifyEmergencyCreatedAsync(string providerUserId, int emergencyId, string serviceName);
        Task NotifyEmergencyAcceptedAsync(string clientUserId, int emergencyId, string providerName);
        Task NotifyReviewReceivedAsync(string providerUserId, int reviewId, int rating);
        Task NotifyProviderApprovedAsync(string userId);
        Task NotifyProviderRejectedAsync(string userId, string reason);
    }
}