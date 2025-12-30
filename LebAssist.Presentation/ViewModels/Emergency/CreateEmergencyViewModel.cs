using LebAssist.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace LebAssist.Presentation.ViewModels.Emergency
{
    public class CreateEmergencyViewModel
    {
        [Required]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }

        [Required]
        [MaxLength(1000)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Location")]
        public string? LocationAddress { get; set; }

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Display(Name = "Photo (Optional)")]
        public IFormFile? Photo { get; set; }

        // For dropdown list - renamed to avoid conflict
        public List<EmergencyServiceSelectItem> Services { get; set; } = new();

        public EmergencyDtos ToDto()
        {
            return new EmergencyDtos
            {
                ServiceId = ServiceId,
                Description = Description,
                LocationAddress = LocationAddress ?? string.Empty,
                Latitude = Latitude,
                Longitude = Longitude
            };
        }
    }

    // Renamed to avoid conflict with Provider.ServiceSelectItem
    public class EmergencyServiceSelectItem
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }

    public class EmergencyDetailsViewModel
    {
        public int EmergencyRequestId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPhone { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? ProviderName { get; set; }
        public string? ProviderPhone { get; set; }
        public string Details { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDateTime { get; set; }
        public DateTime? AcceptedDateTime { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsOwner { get; set; }
        public bool IsProvider { get; set; }
        public bool IsAssignedProvider { get; set; }
    }

    public class EmergencyListViewModel
    {
        public List<EmergencyListItemViewModel> Emergencies { get; set; } = new();
    }

    public class EmergencyListItemViewModel
    {
        public int EmergencyRequestId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDateTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Distance { get; set; }
    }
}