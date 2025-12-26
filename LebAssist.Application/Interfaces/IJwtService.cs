using Microsoft.AspNetCore.Identity;

namespace LebAssist.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(IdentityUser user, IList<string> roles);
    }
}