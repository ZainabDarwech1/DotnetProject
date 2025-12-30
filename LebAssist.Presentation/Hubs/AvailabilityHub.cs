using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LebAssist.Presentation.Hubs
{
    [Authorize]
    public class AvailabilityHub : Hub
    {
        // Provider updates their availability status
        public async Task UpdateAvailability(int providerId, bool isAvailable)
        {
            // Broadcast to all clients viewing providers
            await Clients.All.SendAsync("OnProviderAvailabilityChanged", providerId, isAvailable);
        }

        // Provider updates their location
        public async Task UpdateLocation(int providerId, double latitude, double longitude)
        {
            await Clients.All.SendAsync("OnProviderLocationChanged", providerId, latitude, longitude);
        }

        // Client joins to watch availability updates
        public async Task JoinAvailabilityUpdates()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AvailabilityWatchers");
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // If provider disconnects, mark them as offline
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && Context.User?.IsInRole("Provider") == true)
            {
                // Broadcast that provider went offline
                await Clients.All.SendAsync("OnProviderAvailabilityChanged", userId, false);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}