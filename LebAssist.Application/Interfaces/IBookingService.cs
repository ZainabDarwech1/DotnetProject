using Domain.Entities;
using Domain.Enums;
using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IBookingService
    {
        // Existing methods
        Task<int> CreateBookingAsync(int clientId, BookingDtos dto);
        Task<Booking?> GetBookingDetailsAsync(int bookingId);
        Task<IEnumerable<BookingDetailsDto>> GetProviderBookingsAsync(int providerId);
        Task<bool> AcceptBookingAsync(int bookingId, int providerId);
        Task<bool> RejectBookingAsync(int bookingId, int providerId, string? reason);
        Task<bool> StartBookingAsync(int bookingId, int providerId);
        Task<bool> CompleteBookingAsync(int bookingId, int providerId);
        Task<BookingDetailsDto?> GetBookingByIdAsync(int bookingId);
        Task<IEnumerable<BookingDetailsDto>> GetClientBookingsAsync(int clientId, BookingStatus? status);
        Task<bool> CancelBookingByClientAsync(int bookingId, int clientId, string reason);
    }
}