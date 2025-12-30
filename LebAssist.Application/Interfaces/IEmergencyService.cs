using Domain.Entities;
using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IEmergencyService
    {
        Task<int> CreateEmergencyAsync(int clientId, EmergencyDtos dto);
        Task<bool> AcceptEmergencyAsync(int emergencyId, int providerId);
        Task<bool> DeclineEmergencyAsync(int emergencyId, int providerId);
        Task<bool> StartEmergencyAsync(int emergencyId, int providerId);
        Task<bool> CompleteEmergencyAsync(int emergencyId, int providerId);
        Task<EmergencyRequest?> GetEmergencyDetailsAsync(int emergencyId);
        Task<IEnumerable<EmergencyRequest>> GetClientEmergenciesAsync(int clientId);
        Task<IEnumerable<EmergencyRequest>> GetPendingEmergenciesAsync();
    }
}