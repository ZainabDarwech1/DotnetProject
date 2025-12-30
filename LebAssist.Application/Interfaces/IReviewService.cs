using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IReviewService
    {
        // Create & Update
        Task<ReviewResultDto> CreateReviewAsync(int clientId, CreateReviewDto dto);
        Task<ReviewResultDto> UpdateReviewAsync(int clientId, UpdateReviewDto dto);
        Task<ReviewResultDto> DeleteReviewAsync(int reviewId, int clientId, bool isAdmin = false);

        // Read operations
        Task<ReviewDto?> GetReviewByIdAsync(int reviewId);
        Task<ReviewDto?> GetReviewByBookingIdAsync(int bookingId);
        Task<ProviderReviewsSummaryDto> GetProviderReviewsSummaryAsync(int providerId, int page = 1, int pageSize = 10);
        Task<IEnumerable<ReviewDto>> GetProviderReviewsAsync(int providerId);
        Task<IEnumerable<ReviewDto>> GetClientReviewsAsync(int clientId);
        Task<IEnumerable<ReviewDto>> GetRecentProviderReviewsAsync(int providerId, int count = 5);

        // Rating calculations
        Task<double> GetProviderAverageRatingAsync(int providerId);
        Task<int> GetProviderReviewCountAsync(int providerId);
        Task UpdateProviderRatingAsync(int providerId);

        // Eligibility checks
        Task<ReviewEligibilityDto> CheckReviewEligibilityAsync(int clientId, int bookingId);
        Task<bool> CanClientEditReviewAsync(int clientId, int reviewId);

        // Admin operations
        Task<IEnumerable<AdminReviewDto>> GetAllReviewsForAdminAsync(bool? isVisible = null, bool? isModerated = null);
        Task<ReviewResultDto> ModerateReviewAsync(ModerateReviewDto dto, string adminUserId);
        Task<ReviewResultDto> HideReviewAsync(int reviewId, string adminUserId);
        Task<ReviewResultDto> UnhideReviewAsync(int reviewId, string adminUserId);
    }
}