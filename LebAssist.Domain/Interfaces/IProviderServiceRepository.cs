using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IProviderServiceRepository : IRepository<ProviderServiceEntity>
    {
        Task<IEnumerable<ProviderServiceEntity>> GetByProviderIdAsync(int providerId);
        Task<IEnumerable<ProviderServiceEntity>> GetAllByProviderIdAsync(int providerId);
        Task<IEnumerable<ProviderServiceEntity>> GetByServiceIdAsync(int serviceId);
        Task<ProviderServiceEntity?> GetByProviderAndServiceAsync(int providerId, int serviceId);
    }
}