using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IClientService
    {
        // Existing methods
        Task<ClientRegistrationResult> RegisterAsync(ClientRegistrationDto dto);
        Task<ClientProfileDto?> GetProfileAsync(string aspNetUserId);
        Task<bool> UpdateProfileAsync(string aspNetUserId, UpdateClientProfileDto dto);
        Task<bool> ChangePasswordAsync(string aspNetUserId, string currentPassword, string newPassword);
        Task<string?> UpdateProfilePhotoAsync(string aspNetUserId, byte[] photoData, string fileName);
        Task<bool> DeleteProfilePhotoAsync(string aspNetUserId);

        // NEW - Add these methods
        Task<ClientProfileDto?> GetClientByIdAsync(int clientId);
        Task<ClientProfileDto?> GetClientByAspNetUserIdAsync(string aspNetUserId);

        // List / admin helpers
        Task<IEnumerable<ClientProfileDto>> GetAllClientsAsync();
        Task<IEnumerable<ClientProfileDto>> GetPendingProviderApplicationsAsync();
    }
}