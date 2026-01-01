using LebAssist.Application.DTOs;

namespace LebAssist.Presentation.Areas.Admin.ViewModels
{
    public class PendingApplicationViewModel
    {
        public int ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime DateApplied { get; set; }
    }

    public class ApplicationDetailsViewModel
    {
        public ClientProfileDto? Client { get; set; }
        public List<ProviderServiceDto> Services { get; set; } = new();
        public List<ProviderWorkingHoursDto> WorkingHours { get; set; } = new();
    }
}
