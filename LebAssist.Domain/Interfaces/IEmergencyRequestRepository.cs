using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IEmergencyRequestRepository : IRepository<EmergencyRequest>
    {
        Task<EmergencyRequest?> GetWithDetailsAsync(int emergencyId);
        Task<IEnumerable<EmergencyRequest>> GetByClientIdAsync(int clientId);
        Task<IEnumerable<EmergencyRequest>> GetPendingAsync();
        Task<IEnumerable<EmergencyRequest>> GetByProviderIdAsync(int providerId);
    }
}