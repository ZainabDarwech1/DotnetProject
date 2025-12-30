using Domain.Entities;
using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IBookingService
    {
        // Existing methods
        Task<int> CreateBookingAsync(int clientId, BookingDtos dto);
        Task<Booking?> GetBookingDetailsAsync(int bookingId);
        Task<IEnumerable<BookingDtos>> GetProviderBookingsAsync(int providerId);
        Task<bool> AcceptBookingAsync(int bookingId, int providerId);
        Task<bool> RejectBookingAsync(int bookingId, int providerId, string? reason);
        Task<bool> StartBookingAsync(int bookingId, int providerId);
        Task<bool> CompleteBookingAsync(int bookingId, int providerId);

        // NEW method needed for Reviews
        Task<BookingDetailsDto?> GetBookingByIdAsync(int bookingId);
    }
}