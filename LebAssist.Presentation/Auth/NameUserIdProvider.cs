using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace LebAssist.Presentation.Auth
{
    // Ensures SignalR maps the user identifier to the ASP.NET Identity NameIdentifier claim
    public class NameUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
