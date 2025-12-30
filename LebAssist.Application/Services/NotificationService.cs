using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LebAssist.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> CreateNotificationAsync(
            string userId,
            NotificationType type,
            string title,
            string message,
            int? referenceId = null,
            DateTime? expiryDate = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                ReferenceId = referenceId,
                ExpiryDate = expiryDate,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Notification created for user {UserId}: {Title}", userId, title);

            return notification.NotificationId;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, int take = 20)
        {
            var notifications = await _unitOfWork.Notifications.GetUserNotificationsAsync(userId, take);

            return notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                UserId = n.UserId,
                Type = n.Type,
                ReferenceId = n.ReferenceId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedDate = n.CreatedDate
            });
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            await _unitOfWork.Notifications.DeleteAsync(notificationId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteOldNotificationsAsync()
        {
            await _unitOfWork.Notifications.DeleteOldNotificationsAsync(30);
            await _unitOfWork.Notifications.DeleteExpiredNotificationsAsync();
            _logger.LogInformation("Old notifications cleaned up");
        }

        // ================================
        // Specific Notification Creators
        // ================================

        public async Task NotifyBookingCreatedAsync(string providerUserId, int bookingId, string clientName)
        {
            await CreateNotificationAsync(
                providerUserId,
                NotificationType.Booking,
                "New Booking Request",
                $"You have a new booking request from {clientName}.",
                bookingId
            );
        }

        public async Task NotifyBookingStatusChangedAsync(string clientUserId, int bookingId, string status)
        {
            await CreateNotificationAsync(
                clientUserId,
                NotificationType.Booking,
                "Booking Status Updated",
                $"Your booking #{bookingId} status changed to: {status}",
                bookingId
            );
        }

        public async Task NotifyEmergencyCreatedAsync(string providerUserId, int emergencyId, string serviceName)
        {
            await CreateNotificationAsync(
                providerUserId,
                NotificationType.Emergency,
                "🚨 Emergency Request!",
                $"New emergency request for {serviceName}. Respond now!",
                emergencyId,
                DateTime.UtcNow.AddHours(2)
            );
        }

        public async Task NotifyEmergencyAcceptedAsync(string clientUserId, int emergencyId, string providerName)
        {
            await CreateNotificationAsync(
                clientUserId,
                NotificationType.Emergency,
                "Emergency Accepted!",
                $"Your emergency has been accepted by {providerName}. They are on the way!",
                emergencyId
            );
        }

        public async Task NotifyReviewReceivedAsync(string providerUserId, int reviewId, int rating)
        {
            var stars = new string('⭐', rating);
            await CreateNotificationAsync(
                providerUserId,
                NotificationType.Review,
                "New Review Received",
                $"You received a {rating}-star review! {stars}",
                reviewId
            );
        }

        public async Task NotifyProviderApprovedAsync(string userId)
        {
            await CreateNotificationAsync(
                userId,
                NotificationType.Admin,
                "Application Approved! 🎉",
                "Congratulations! Your provider application has been approved. You can now start accepting jobs.",
                null
            );
        }

        public async Task NotifyProviderRejectedAsync(string userId, string reason)
        {
            await CreateNotificationAsync(
                userId,
                NotificationType.Admin,
                "Application Status",
                $"Your provider application was not approved. Reason: {reason}",
                null
            );
        }
    }
}