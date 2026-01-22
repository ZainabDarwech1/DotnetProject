using Domain.Enums;
using LebAssist.Application.Interfaces;
using LebAssist.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LebAssist.Presentation.Services;

/// <summary>
/// Decorates the application NotificationService to additionally fan-out realtime events via SignalR.
/// Keeps SignalR dependency in Presentation.
/// </summary>
public sealed class SignalRNotificationServiceDecorator : INotificationService
{
    private readonly INotificationService _inner;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<SignalRNotificationServiceDecorator> _logger;

    public SignalRNotificationServiceDecorator(
        INotificationService inner,
        IHubContext<NotificationHub> hub,
        ILogger<SignalRNotificationServiceDecorator> logger)
    {
        _inner = inner;
        _hub = hub;
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
        var notificationId = await _inner.CreateNotificationAsync(
            userId, type, title, message, referenceId, expiryDate);

        try
        {
            var unreadCount = await _inner.GetUnreadCountAsync(userId);

            await _hub.Clients
                .User(userId)
                .SendAsync("ReceiveNotification", new
                {
                    notificationId,
                    title,
                    message,
                    type = (int)type,
                    typeName = type.ToString(),
                    referenceId,
                    unreadCount,
                    createdDate = DateTime.UtcNow
                });

            _logger.LogInformation("SignalR notification sent to user {UserId}: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "SignalR notification fan-out failed for user {UserId}", userId);
        }

        return notificationId;
    }

    public Task<IEnumerable<LebAssist.Application.DTOs.NotificationDto>> GetUserNotificationsAsync(string userId, int take = 20)
        => _inner.GetUserNotificationsAsync(userId, take);

    public Task<int> GetUnreadCountAsync(string userId)
        => _inner.GetUnreadCountAsync(userId);

    public Task MarkAsReadAsync(int notificationId)
        => _inner.MarkAsReadAsync(notificationId);

    public Task MarkAllAsReadAsync(string userId)
        => _inner.MarkAllAsReadAsync(userId);

    public Task DeleteNotificationAsync(int notificationId)
        => _inner.DeleteNotificationAsync(notificationId);

    public Task DeleteOldNotificationsAsync()
        => _inner.DeleteOldNotificationsAsync();

    public Task NotifyBookingCreatedAsync(string providerUserId, int bookingId, string clientName)
        => _inner.NotifyBookingCreatedAsync(providerUserId, bookingId, clientName);

    public Task NotifyBookingStatusChangedAsync(string clientUserId, int bookingId, string status)
        => _inner.NotifyBookingStatusChangedAsync(clientUserId, bookingId, status);

    public Task NotifyEmergencyCreatedAsync(string providerUserId, int emergencyId, string serviceName)
        => _inner.NotifyEmergencyCreatedAsync(providerUserId, emergencyId, serviceName);

    public Task NotifyEmergencyAcceptedAsync(string clientUserId, int emergencyId, string providerName)
        => _inner.NotifyEmergencyAcceptedAsync(clientUserId, emergencyId, providerName);

    public Task NotifyReviewReceivedAsync(string providerUserId, int reviewId, int rating)
        => _inner.NotifyReviewReceivedAsync(providerUserId, reviewId, rating);

    public Task NotifyProviderApprovedAsync(string userId)
        => _inner.NotifyProviderApprovedAsync(userId);

    public Task NotifyProviderRejectedAsync(string userId, string reason)
        => _inner.NotifyProviderRejectedAsync(userId, reason);
}
