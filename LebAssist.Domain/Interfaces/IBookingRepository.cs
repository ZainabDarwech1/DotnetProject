using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetClientBookingsAsync(int clientId);
        Task<IEnumerable<Booking>> GetProviderBookingsAsync(int providerId);
        Task<IEnumerable<Booking>> GetPendingBookingsAsync(int providerId);
        Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status);
        Task<Booking?> GetBookingWithDetailsAsync(int bookingId);
    }
}