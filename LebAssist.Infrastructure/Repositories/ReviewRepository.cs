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

        public async Task<IEnumerable<Review>> GetProviderReviewsAsync(int providerId, bool includeHidden = false)
        {
            var query = _dbSet
                .Include(r => r.Client)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .Where(r => r.ProviderId == providerId);

            if (!includeHidden)
            {
                query = query.Where(r => r.IsVisible);
            }

            return await query
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetProviderReviewsPagedAsync(int providerId, int page, int pageSize)
        {
            return await _dbSet
                .Include(r => r.Client)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .Where(r => r.ProviderId == providerId && r.IsVisible)
                .OrderByDescending(r => r.ReviewDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                .Include(r => r.Client)
                .Include(r => r.Provider)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.BookingId == bookingId);
        }

        public async Task<int> GetReviewCountAsync(int providerId)
        {
            return await _dbSet
                .CountAsync(r => r.ProviderId == providerId && r.IsVisible);
        }

        public async Task<Review?> GetReviewWithDetailsAsync(int reviewId)
        {
            return await _dbSet
                .Include(r => r.Client)
                .Include(r => r.Provider)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int providerId)
        {
            var reviews = await _dbSet
                .Where(r => r.ProviderId == providerId && r.IsVisible)
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            var distribution = new Dictionary<int, int>
            {
                { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
            };

            foreach (var item in reviews)
            {
                distribution[item.Rating] = item.Count;
            }

            return distribution;
        }

        public async Task<IEnumerable<Review>> GetRecentReviewsAsync(int providerId, int count)
        {
            return await _dbSet
                .Include(r => r.Client)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .Where(r => r.ProviderId == providerId && r.IsVisible)
                .OrderByDescending(r => r.ReviewDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetAllReviewsForAdminAsync(bool? isVisible = null, bool? isModerated = null)
        {
            var query = _dbSet
                .Include(r => r.Client)
                .Include(r => r.Provider)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .AsQueryable();

            if (isVisible.HasValue)
            {
                query = query.Where(r => r.IsVisible == isVisible.Value);
            }

            if (isModerated.HasValue)
            {
                query = query.Where(r => r.AdminModerated == isModerated.Value);
            }

            return await query
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetClientReviewsAsync(int clientId)
        {
            return await _dbSet
                .Include(r => r.Provider)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .Where(r => r.ClientId == clientId)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }

        public async Task<bool> HasClientReviewedBookingAsync(int clientId, int bookingId)
        {
            return await _dbSet
                .AnyAsync(r => r.ClientId == clientId && r.BookingId == bookingId);
        }
    }
}