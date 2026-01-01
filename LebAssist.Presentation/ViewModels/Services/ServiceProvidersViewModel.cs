using LebAssist.Application.DTOs;

namespace LebAssist.Presentation.ViewModels.Services
{
    public class ProviderOfferingViewModel
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string? ProviderPhotoPath { get; set; }
        public int ProviderServiceId { get; set; }
        public decimal PricePerHour { get; set; }
    }

    public class ServiceProvidersViewModel
    {
        public ServiceDto Service { get; set; } = null!;
        public List<ProviderOfferingViewModel> Providers { get; set; } = new();
    }
}
