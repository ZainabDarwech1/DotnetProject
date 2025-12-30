using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IProviderService
    {
        // Provider Application
        Task<bool> ApplyAsProviderAsync(string userId, ProviderApplicationDto dto);

        // Existing methods
        Task<IEnumerable<ProviderServiceDto>> GetProviderServicesAsync(int clientId);
        Task<bool> AddProviderServiceAsync(int clientId, AddProviderServiceDto dto);
        Task<bool> RemoveProviderServiceAsync(int clientId, int serviceId);
        Task<bool> UpdateProviderServicePriceAsync(int clientId, int serviceId, decimal pricePerHour);
        Task<IEnumerable<PortfolioPhotoDto>> GetProviderPortfolioAsync(int clientId);
        Task<int> AddPortfolioPhotoAsync(int clientId, byte[] photoData, string fileName, string? caption);
        Task<bool> DeletePortfolioPhotoAsync(int clientId, int portfolioPhotoId);
        Task<bool> ReorderPortfolioPhotosAsync(int clientId, List<int> photoIds);
        // Add this method to your existing IProviderService interface
        Task<bool> IsProviderAvailableAsync(int providerId);
    }
}