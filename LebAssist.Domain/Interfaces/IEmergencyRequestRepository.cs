using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IEmergencyRequestRepository : IRepository<EmergencyRequest>
    {
        Task<IEnumerable<EmergencyRequest>> GetActiveEmergencyRequestsAsync();
        Task<EmergencyRequest?> GetEmergencyByIdWithDetailsAsync(int id);
        Task<IEnumerable<EmergencyRequest>> GetClientEmergenciesAsync(int clientId);
        Task<IEnumerable<EmergencyRequest>> GetProviderEmergenciesAsync(int providerId);
    }
}