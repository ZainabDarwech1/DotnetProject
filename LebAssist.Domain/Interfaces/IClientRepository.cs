using Domain.Entities;


namespace Domain.Interfaces
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client?> GetByAspNetUserIdAsync(string userId);
        Task<IEnumerable<Client>> GetProvidersNearLocationAsync(decimal lat, decimal lon, int radiusKm);
        Task<IEnumerable<Client>> GetProvidersByServiceAsync(int serviceId);
        Task<IEnumerable<Client>> GetActiveProvidersAsync();
    }
}