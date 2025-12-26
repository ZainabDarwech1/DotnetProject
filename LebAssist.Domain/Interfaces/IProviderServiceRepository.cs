using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IProviderServiceRepository : IRepository<ProviderService>
    {
        Task<IEnumerable<ProviderService>> GetByProviderIdAsync(int providerId);
        Task<IEnumerable<ProviderService>> GetByServiceIdAsync(int serviceId);
        Task<ProviderService?> GetByProviderAndServiceAsync(int providerId, int serviceId);
    }
}