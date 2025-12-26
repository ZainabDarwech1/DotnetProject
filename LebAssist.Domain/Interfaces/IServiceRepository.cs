using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IServiceRepository : IRepository<Service>
    {
        Task<IEnumerable<Service>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Service>> GetActiveServicesAsync();
        Task<Service?> GetServiceWithProvidersAsync(int serviceId);
    }
}