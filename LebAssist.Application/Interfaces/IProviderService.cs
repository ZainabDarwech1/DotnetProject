using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IProviderService
    {
        // Provider Application
        Task<bool> ApplyAsProviderAsync(string userId, ProviderApplicationDto dto);
        Task<string?> SaveProfilePhotoAsync(string userId, byte[] photoData, string fileName);

        // Approval workflow
        Task<bool> ApproveProviderAsync(int clientId, string adminUserId);
        Task<bool> RejectProviderAsync(int clientId, string reason, string adminUserId);

        // Read helpers
        Task<IEnumerable<ProviderServiceDto>> GetProviderServicesAsync(int clientId);
        Task<IEnumerable<ProviderWorkingHoursDto>> GetProviderWorkingHoursAsync(int clientId);

        // Existing methods
        Task<bool> AddProviderServiceAsync(int clientId, AddProviderServiceDto dto);
        Task<bool> RemoveProviderServiceAsync(int clientId, int serviceId);
        Task<bool> UpdateProviderServicePriceAsync(int clientId, int serviceId, decimal pricePerHour);
        
        // NEW: Working hours management
        Task<bool> AddProviderServiceWithHoursAsync(int clientId, AddProviderServiceDto dto, List<ProviderWorkingHoursDto> workingHours);
        Task<bool> UpdateProviderServiceAsync(int clientId, int serviceId, decimal pricePerHour, List<ProviderWorkingHoursDto> workingHours);
        Task<IEnumerable<ProviderWorkingHoursDto>> GetServiceWorkingHoursAsync(int clientId, int serviceId);

        Task<IEnumerable<PortfolioPhotoDto>> GetProviderPortfolioAsync(int clientId);
        Task<int> AddPortfolioPhotoAsync(int clientId, byte[] photoData, string fileName, string? caption);
        Task<bool> DeletePortfolioPhotoAsync(int clientId, int portfolioPhotoId);
        Task<bool> ReorderPortfolioPhotosAsync(int clientId, List<int> photoIds);
        Task<bool> IsProviderAvailableAsync(int providerId);

        // NEW: Get providers offering a service
        Task<IEnumerable<ProviderOfferingDto>> GetProvidersByServiceIdAsync(int serviceId);
    }
}