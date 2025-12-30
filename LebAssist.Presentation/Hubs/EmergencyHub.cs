using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LebAssist.Presentation.Hubs
{
    [Authorize]
    public class EmergencyHub : Hub
    {
        // Provider joins the "Providers" group to receive emergency alerts
        public async Task JoinProviders()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Providers");
        }

        // Provider leaves the "Providers" group
        public async Task LeaveProviders()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Providers");
        }

        // Server sends emergency alert to all providers
        public async Task SendEmergencyAlert(object emergency)
        {
            await Clients.Group("Providers").SendAsync("OnEmergencyReceived", emergency);
        }

        // Server notifies that emergency was removed (accepted by someone)
        public async Task RemoveEmergency(int emergencyId)
        {
            await Clients.Group("Providers").SendAsync("OnEmergencyRemoved", emergencyId);
        }

        // Server notifies that emergency was accepted
        public async Task EmergencyAccepted(int emergencyId, string providerName)
        {
            await Clients.Group("Providers").SendAsync("OnEmergencyAccepted", emergencyId, providerName);
        }

        // Notify specific client about their emergency status
        public async Task NotifyClient(string clientUserId, int emergencyId, string status, string? providerName)
        {
            await Clients.User(clientUserId).SendAsync("OnEmergencyStatusChanged", new
            {
                emergencyId,
                status,
                providerName
            });
        }

        public override async Task OnConnectedAsync()
        {
            // Auto-join providers group if user is a provider
            var user = Context.User;
            if (user?.IsInRole("Provider") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Providers");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Providers");
            await base.OnDisconnectedAsync(exception);
        }
    }
}