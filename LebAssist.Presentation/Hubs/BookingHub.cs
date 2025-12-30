using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LebAssist.Presentation.Hubs
{
    [Authorize]
    public class BookingHub : Hub
    {
        // Notify user about booking status change
        public async Task NotifyBookingStatusChange(string userId, int bookingId, string newStatus)
        {
            await Clients.User(userId).SendAsync("OnBookingStatusChanged", new
            {
                bookingId,
                status = newStatus,
                timestamp = DateTime.UtcNow
            });
        }

        // Notify provider about new booking request
        public async Task NotifyNewBookingRequest(string providerUserId, object booking)
        {
            await Clients.User(providerUserId).SendAsync("OnNewBookingRequest", booking);
        }

        // Notify client that booking was accepted
        public async Task NotifyBookingAccepted(string clientUserId, int bookingId, string providerName)
        {
            await Clients.User(clientUserId).SendAsync("OnBookingAccepted", new
            {
                bookingId,
                providerName,
                message = $"Your booking has been accepted by {providerName}!"
            });
        }

        // Notify client that booking was rejected
        public async Task NotifyBookingRejected(string clientUserId, int bookingId, string? reason)
        {
            await Clients.User(clientUserId).SendAsync("OnBookingRejected", new
            {
                bookingId,
                reason = reason ?? "No reason provided",
                message = "Your booking has been rejected."
            });
        }

        // Notify client that service has started
        public async Task NotifyBookingStarted(string clientUserId, int bookingId)
        {
            await Clients.User(clientUserId).SendAsync("OnBookingStarted", new
            {
                bookingId,
                message = "Your service has started!"
            });
        }

        // Notify client that service is completed
        public async Task NotifyBookingCompleted(string clientUserId, int bookingId)
        {
            await Clients.User(clientUserId).SendAsync("OnBookingCompleted", new
            {
                bookingId,
                message = "Your service has been completed! Please leave a review."
            });
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}