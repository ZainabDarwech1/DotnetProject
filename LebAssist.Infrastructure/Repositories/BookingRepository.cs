using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LebAssist.Infrastructure.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Booking>> GetClientBookingsAsync(int clientId)
        {
            return await _dbSet
                .Include(b => b.Provider)
                .Include(b => b.Service)
                .Where(b => b.ClientId == clientId)
                .OrderByDescending(b => b.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetProviderBookingsAsync(int providerId)
        {
            return await _dbSet
                .Include(b => b.Client)
                .Include(b => b.Service)
                .Where(b => b.ProviderId == providerId)
                .OrderByDescending(b => b.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetPendingBookingsAsync(int providerId)
        {
            return await _dbSet
                .Include(b => b.Client)
                .Include(b => b.Service)
                .Where(b => b.ProviderId == providerId && b.Status == BookingStatus.Pending)
                .OrderBy(b => b.ScheduledDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status)
        {
            return await _dbSet
                .Include(b => b.Client)
                .Include(b => b.Provider)
                .Include(b => b.Service)
                .Where(b => b.Status == status)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingWithDetailsAsync(int bookingId)
        {
            return await _dbSet
                .Include(b => b.Client)
                .Include(b => b.Provider)
                .Include(b => b.Service)
                .Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }
    }
}