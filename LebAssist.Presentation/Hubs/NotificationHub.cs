using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LebAssist.Presentation.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // Send notification to specific user
        public async Task SendNotificationToUser(string userId, object notification)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }

        // Send notification to multiple users
        public async Task SendNotificationToUsers(List<string> userIds, object notification)
        {
            await Clients.Users(userIds).SendAsync("ReceiveNotification", notification);
        }

        // Broadcast notification to all users
        public async Task BroadcastNotification(object notification)
        {
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }

        // Mark notification as read
        public async Task MarkAsRead(int notificationId)
        {
            // This would typically update the database
            // For now, just acknowledge to the client
            await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
        }

        // Request unread count
        public async Task RequestUnreadCount()
        {
            // This would typically query the database
            // For now, send a placeholder
            await Clients.Caller.SendAsync("UnreadCountUpdated", 0);
        }

        public override async Task OnConnectedAsync()
        {
            // Map connected user into a stable group so server-side code can target them even if IUserIdProvider is inconsistent.
            var userId = Context.UserIdentifier ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var groupName = GetUserGroup(userId);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                // optional: send initial unread count or log
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var groupName = GetUserGroup(userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private static string GetUserGroup(string userId) => $"u:{userId}";
    }
}