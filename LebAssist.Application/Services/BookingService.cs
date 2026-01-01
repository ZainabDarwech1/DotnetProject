using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;

namespace LebAssist.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =========================
        // R7.3 – Create Booking
        // =========================
        public async Task<int> CreateBookingAsync(int clientId, BookingDtos dto)
        {
            var booking = new Booking
            {
                ClientId = clientId,
                ProviderId = dto.ProviderId,
                ServiceId = dto.ServiceId,
                ScheduledDateTime = dto.BookingDateTime,
                Latitude = (decimal)dto.Latitude,
                Longitude = (decimal)dto.Longitude,
                Notes = dto.Notes,
                Status = BookingStatus.Pending
            };

            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            return booking.BookingId;
        }

        // =========================
        // R7.4 – Provider View Bookings
        // =========================
        public async Task<IEnumerable<BookingDetailsDto>> GetProviderBookingsAsync(int providerId)
        {
            var bookings = await _unitOfWork.Bookings.GetProviderBookingsAsync(providerId);

            return bookings.Select(b => new BookingDetailsDto
            {
                BookingId = b.BookingId,
                ClientId = b.ClientId,
                ClientName = b.Client != null ? $"{b.Client.FirstName} {b.Client.LastName}" : "Unknown",
                ClientPhotoPath = b.Client?.ProfilePhotoPath,
                ClientPhone = b.Client?.PhoneNumber,
                ProviderId = b.ProviderId,
                ProviderName = b.Provider != null ? $"{b.Provider.FirstName} {b.Provider.LastName}" : "Unknown",
                ProviderPhotoPath = b.Provider?.ProfilePhotoPath,
                ServiceId = b.ServiceId,
                ServiceName = b.Service?.ServiceName ?? "Unknown",
                CategoryName = b.Service?.Category?.CategoryName ?? string.Empty,
                RequestDate = b.RequestDate,
                ScheduledDateTime = b.ScheduledDateTime,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                Status = b.Status,
                Notes = b.Notes,
                CompletedDate = b.CompletedDate,
                CancellationReason = b.CancellationReason,
                HasReview = b.Review != null,
                ReviewId = b.Review?.ReviewId
            });
        }

        // =========================
        // R7.4 – Accept Booking
        // =========================
        public async Task<bool> AcceptBookingAsync(int bookingId, int providerId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);

            if (booking == null || booking.ProviderId != providerId)
                return false;

            if (booking.Status != BookingStatus.Pending)
                return false;

            booking.Status = BookingStatus.Accepted;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // =========================
        // R7.4 – Reject Booking
        // =========================
        public async Task<bool> RejectBookingAsync(int bookingId, int providerId, string? reason)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);

            if (booking == null || booking.ProviderId != providerId)
                return false;

            if (booking.Status != BookingStatus.Pending)
                return false;

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = reason;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // =========================
        // Start Booking
        // =========================
        public async Task<bool> StartBookingAsync(int bookingId, int providerId)
        {
            var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);

            if (booking == null || booking.ProviderId != providerId)
                return false;

            if (booking.Status != BookingStatus.Accepted)
                return false;

            booking.Status = BookingStatus.InProgress;
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // =========================
        // Complete Booking
        // =========================
        public async Task<bool> CompleteBookingAsync(int bookingId, int providerId)
        {
            var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);

            if (booking == null || booking.ProviderId != providerId)
                return false;

            if (booking.Status != BookingStatus.InProgress)
                return false;

            booking.Status = BookingStatus.Completed;
            booking.CompletedDate = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // =========================
        // Get Booking Details - FIXED: Returns nullable
        // =========================
        public async Task<Booking?> GetBookingDetailsAsync(int bookingId)
        {
            return await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
        }
        public async Task<BookingDetailsDto?> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking == null) return null;

            // Load related data
            var client = await _unitOfWork.Clients.GetByIdAsync(booking.ClientId);
            var provider = await _unitOfWork.Clients.GetByIdAsync(booking.ProviderId);
            var service = await _unitOfWork.Services.GetByIdAsync(booking.ServiceId);
            var review = await _unitOfWork.Reviews.GetReviewByBookingIdAsync(bookingId);

            string categoryName = string.Empty;
            if (service != null)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(service.CategoryId);
                categoryName = category?.CategoryName ?? string.Empty;
            }

            return new BookingDetailsDto
            {
                BookingId = booking.BookingId,
                ClientId = booking.ClientId,
                ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "Unknown",
                ClientPhotoPath = client?.ProfilePhotoPath,
                ProviderId = booking.ProviderId,
                ProviderName = provider != null ? $"{provider.FirstName} {provider.LastName}" : "Unknown",
                ProviderPhotoPath = provider?.ProfilePhotoPath,
                ServiceId = booking.ServiceId,
                ServiceName = service?.ServiceName ?? "Unknown Service",
                CategoryName = categoryName,
                RequestDate = booking.RequestDate,
                ScheduledDateTime = booking.ScheduledDateTime,
                Latitude = booking.Latitude,
                Longitude = booking.Longitude,
                Status = booking.Status,
                Notes = booking.Notes,
                CompletedDate = booking.CompletedDate,
                CancellationReason = booking.CancellationReason,
                HasReview = review != null,
                ReviewId = review?.ReviewId
            };
        }

        public async Task<IEnumerable<BookingDetailsDto>> GetClientBookingsAsync(int clientId, BookingStatus? status)
        {
            var bookings = await _unitOfWork.Bookings.GetClientBookingsAsync(clientId);

            if (status.HasValue)
                bookings = bookings.Where(b => b.Status == status);

            return bookings.Select(b => new BookingDetailsDto
            {
                BookingId = b.BookingId,
                ProviderName = $"{b.Provider.FirstName} {b.Provider.LastName}",
                ProviderPhotoPath = b.Provider.ProfilePhotoPath,
                ServiceName = b.Service.ServiceName,
                ScheduledDateTime = b.ScheduledDateTime,
                Status = b.Status
            });
        }

        public async Task<bool> CancelBookingByClientAsync(
            int bookingId, int clientId, string reason)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);

            if (booking == null || booking.ClientId != clientId)
                return false;

            if (booking.Status != BookingStatus.Pending &&
                booking.Status != BookingStatus.Accepted)
                return false;

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = reason;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}