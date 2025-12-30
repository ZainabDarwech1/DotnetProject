using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetProviderReviewsAsync(int providerId, bool includeHidden = false);
        Task<IEnumerable<Review>> GetProviderReviewsPagedAsync(int providerId, int page, int pageSize);
        Task<double> GetAverageRatingAsync(int providerId);
        Task<Review?> GetReviewByBookingIdAsync(int bookingId);
        Task<int> GetReviewCountAsync(int providerId);
        Task<Review?> GetReviewWithDetailsAsync(int reviewId);
        Task<Dictionary<int, int>> GetRatingDistributionAsync(int providerId);
        Task<IEnumerable<Review>> GetRecentReviewsAsync(int providerId, int count);
        Task<IEnumerable<Review>> GetAllReviewsForAdminAsync(bool? isVisible = null, bool? isModerated = null);
        Task<IEnumerable<Review>> GetClientReviewsAsync(int clientId);
        Task<bool> HasClientReviewedBookingAsync(int clientId, int bookingId);
    }
}