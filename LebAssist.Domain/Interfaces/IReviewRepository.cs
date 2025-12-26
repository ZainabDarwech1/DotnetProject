using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetProviderReviewsAsync(int providerId);
        Task<double> GetAverageRatingAsync(int providerId);
        Task<Review?> GetReviewByBookingIdAsync(int bookingId);
        Task<int> GetReviewCountAsync(int providerId);
    }
}