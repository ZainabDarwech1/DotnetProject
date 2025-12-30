using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IServiceService
    {
        Task<IEnumerable<ServiceDto>> GetAllServicesAsync();
        Task<IEnumerable<ServiceDto>> GetActiveServicesAsync();
        Task<IEnumerable<ServiceDto>> GetServicesByCategoryAsync(int categoryId);
        Task<ServiceDto?> GetServiceByIdAsync(int serviceId);  // Changed from GetByIdAsync
        Task<int> CreateServiceAsync(CreateServiceDto dto);
        Task<bool> UpdateServiceAsync(int serviceId, UpdateServiceDto dto);
        Task<bool> DeleteServiceAsync(int serviceId);
        Task<bool> ToggleServiceStatusAsync(int serviceId);
    }
}