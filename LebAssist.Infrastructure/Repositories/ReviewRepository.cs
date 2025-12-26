using Domain.Entities;
using Domain.Interfaces;
using LebAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LebAssist.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Review>> GetProviderReviewsAsync(int providerId)
        {
            return await _dbSet
                .Include(r => r.Client)
                .Include(r => r.Booking)
                .Where(r => r.ProviderId == providerId && r.IsVisible)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(int providerId)
        {
            var reviews = await _dbSet
                .Where(r => r.ProviderId == providerId && r.IsVisible)
                .ToListAsync();

            if (!reviews.Any())
                return 0;

            return reviews.Average(r => r.Rating);
        }

        public async Task<Review?> GetReviewByBookingIdAsync(int bookingId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.BookingId == bookingId);
        }

        public async Task<int> GetReviewCountAsync(int providerId)
        {
            return await _dbSet
                .CountAsync(r => r.ProviderId == providerId && r.IsVisible);
        }
    }
}