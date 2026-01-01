using LebAssist.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace LebAssist.Presentation.ViewModels.Booking
{
    public class CreateBookingViewModel
    {
        [Required]
        public int ProviderId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime BookingDateTime { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string? Notes { get; set; }

        public BookingDtos ToDto()
        {
            return new BookingDtos
            {
                ProviderId = ProviderId,
                ServiceId = ServiceId,
                BookingDateTime = BookingDateTime,
                Latitude = Latitude,
                Longitude = Longitude,
                Notes = Notes
            };
        }
    }
}