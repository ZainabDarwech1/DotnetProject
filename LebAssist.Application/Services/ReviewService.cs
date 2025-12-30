using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LebAssist.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReviewService> _logger;

        // Configuration: Number of days allowed to edit a review
        private const int EditWindowDays = 7;

        public ReviewService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            ILogger<ReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        #region Create & Update

        public async Task<ReviewResultDto> CreateReviewAsync(int clientId, CreateReviewDto dto)
        {
            try
            {
                // Check eligibility
                var eligibility = await CheckReviewEligibilityAsync(clientId, dto.BookingId);
                if (!eligibility.CanReview)
                {
                    return new ReviewResultDto
                    {
                        Success = false,
                        Message = eligibility.Reason ?? "You cannot review this booking.",
                        Errors = new List<string> { eligibility.Reason ?? "Ineligible to review" }
                    };
                }

                // Get booking details
                var booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId);
                if (booking == null)
                {
                    return new ReviewResultDto
                    {
                        Success = false,
                        Message = "Booking not found.",
                        Errors = new List<string> { "Booking not found" }
                    };
                }

                // Create review entity
                var review = new Review
                {
                    BookingId = dto.BookingId,
                    ClientId = clientId,
                    ProviderId = booking.ProviderId,
                    Rating = dto.Rating,
                    Comment = dto.Comment?.Trim(),
                    IsAnonymous = dto.IsAnonymous,
                    ReviewDate = DateTime.UtcNow,
                    IsVisible = true,
                    AdminModerated = false
                };

                await _unitOfWork.Reviews.AddAsync(review);
                await _unitOfWork.SaveChangesAsync();

                // Update provider's average rating
                await UpdateProviderRatingAsync(booking.ProviderId);

                // Send notification to provider
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                var clientName = dto.IsAnonymous ? "A client" : $"{client?.FirstName} {client?.LastName}";

                var provider = await _unitOfWork.Clients.GetByIdAsync(booking.ProviderId);
                if (provider != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        provider.AspNetUserId,
                        NotificationType.Review,
                        $"{clientName} left you a {dto.Rating}-star review!",
                        review.ReviewId
                    );
                }

                _logger.LogInformation("Review {ReviewId} created by client {ClientId} for provider {ProviderId}",
                    review.ReviewId, clientId, booking.ProviderId);

                return new ReviewResultDto
                {
                    Success = true,
                    Message = "Review submitted successfully!",
                    ReviewId = review.ReviewId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for booking {BookingId}", dto.BookingId);
                return new ReviewResultDto
                {
                    Success = false,
                    Message = "An error occurred while submitting your review.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ReviewResultDto> UpdateReviewAsync(int clientId, UpdateReviewDto dto)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetReviewWithDetailsAsync(dto.ReviewId);
                if (review == null)
                {
                    return new ReviewResultDto
                    {
                        Success = false,
                        Message = "Review not found.",
                        Errors = new List<string> { "Review not found" }
                    };
                }

                // Verify ownership
                if (review.ClientId != clientId)
                {
                    return new ReviewResultDto
                    {
                        Success = false,
                        Message = "You can only edit your own reviews.",
                        Errors = new List<string> { "Unauthorized" }
                    };
                }

                // Check if within edit window
                if (!await CanClientEditReviewAsync(clientId, dto.ReviewId))
                {
                    return new ReviewResultDto
                    {
                        Success = false,
                        Message = $"Reviews can only be edited within {EditWindowDays} days of submission.",
                        Errors = new List<string> { "Edit window expired" }
                    };
                }

                // Update review
                review.Rating = dto.Rating;
                review.Comment = dto.Comment?.Trim();
                review.IsAnonymous = dto.IsAnonymous;

                await _unitOfWork.Reviews.UpdateAsync(review);
                await _unitOfWork.SaveChangesAsync();

                // Update provider's average rating
                await UpdateProviderRatingAsync(review.ProviderId);

                _logger.LogInformation("Review {ReviewId} updated by client {ClientId}", dto.ReviewId, clientId);

                return new ReviewResultDto
                {
                    Success = true,
                    Message = "Review updated successfully!",
                    ReviewId = review.ReviewId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", dto.ReviewId);
                return new ReviewResultDto
                {
                    Success = false,
                    Message = "An error occurred while updating your review.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ReviewResultDto> DeleteReviewAsync(int reviewId, int clientId, bool isAdmin = false)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
                if (review == null)
                {
                    return new ReviewResultDto
                    {
                        Success = false,
                        Message = "Review not found.",
                        Errors = new List<string> { "Review not found" }
                    };
                }

                // Verify ownership or admin
                if (!isAdmin && review.ClientId != clientId)
                {
                    return new ReviewResultDto
                    {
                        Success = false,
                        Message = "You can only delete your own reviews.",
                        Errors = new List<string> { "Unauthorized" }
                    };
                }

                var providerId = review.ProviderId;

                await _unitOfWork.Reviews.DeleteAsync(reviewId);
                await _unitOfWork.SaveChangesAsync();

                // Update provider's average rating
                await UpdateProviderRatingAsync(providerId);

                _logger.LogInformation("Review {ReviewId} deleted by {Actor}", reviewId, isAdmin ? "admin" : $"client {clientId}");

                return new ReviewResultDto
                {
                    Success = true,
                    Message = "Review deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
                return new ReviewResultDto
                {
                    Success = false,
                    Message = "An error occurred while deleting the review.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        #endregion

        #region Read Operations

        public async Task<ReviewDto?> GetReviewByIdAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetReviewWithDetailsAsync(reviewId);
            return review == null ? null : MapToDto(review);
        }

        public async Task<ReviewDto?> GetReviewByBookingIdAsync(int bookingId)
        {
            var review = await _unitOfWork.Reviews.GetReviewByBookingIdAsync(bookingId);
            return review == null ? null : MapToDto(review);
        }

        public async Task<ProviderReviewsSummaryDto> GetProviderReviewsSummaryAsync(int providerId, int page = 1, int pageSize = 10)
        {
            var provider = await _unitOfWork.Clients.GetByIdAsync(providerId);
            var totalReviews = await _unitOfWork.Reviews.GetReviewCountAsync(providerId);
            var averageRating = await _unitOfWork.Reviews.GetAverageRatingAsync(providerId);
            var distribution = await _unitOfWork.Reviews.GetRatingDistributionAsync(providerId);
            var reviews = await _unitOfWork.Reviews.GetProviderReviewsPagedAsync(providerId, page, pageSize);

            var totalPages = (int)Math.Ceiling((double)totalReviews / pageSize);

            // Calculate percentages
            var percentages = new Dictionary<int, double>();
            foreach (var kvp in distribution)
            {
                percentages[kvp.Key] = totalReviews > 0
                    ? Math.Round((double)kvp.Value / totalReviews * 100, 1)
                    : 0;
            }

            return new ProviderReviewsSummaryDto
            {
                ProviderId = providerId,
                ProviderName = provider != null ? $"{provider.FirstName} {provider.LastName}" : "Unknown",
                AverageRating = Math.Round(averageRating, 1),
                TotalReviews = totalReviews,
                RatingDistribution = distribution,
                RatingPercentages = percentages,
                Reviews = reviews.Select(r => MapToDto(r)).ToList(),
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<ReviewDto>> GetProviderReviewsAsync(int providerId)
        {
            var reviews = await _unitOfWork.Reviews.GetProviderReviewsAsync(providerId);
            return reviews.Select(r => MapToDto(r));
        }

        public async Task<IEnumerable<ReviewDto>> GetClientReviewsAsync(int clientId)
        {
            var reviews = await _unitOfWork.Reviews.GetClientReviewsAsync(clientId);
            return reviews.Select(r => MapToDto(r));
        }

        public async Task<IEnumerable<ReviewDto>> GetRecentProviderReviewsAsync(int providerId, int count = 5)
        {
            var reviews = await _unitOfWork.Reviews.GetRecentReviewsAsync(providerId, count);
            return reviews.Select(r => MapToDto(r));
        }

        #endregion

        #region Rating Calculations

        public async Task<double> GetProviderAverageRatingAsync(int providerId)
        {
            return await _unitOfWork.Reviews.GetAverageRatingAsync(providerId);
        }

        public async Task<int> GetProviderReviewCountAsync(int providerId)
        {
            return await _unitOfWork.Reviews.GetReviewCountAsync(providerId);
        }

        public async Task UpdateProviderRatingAsync(int providerId)
        {
            try
            {
                var provider = await _unitOfWork.Clients.GetByIdAsync(providerId);
                if (provider == null) return;

                var averageRating = await _unitOfWork.Reviews.GetAverageRatingAsync(providerId);
                var totalReviews = await _unitOfWork.Reviews.GetReviewCountAsync(providerId);

                provider.ProviderAverageRating = (decimal)Math.Round(averageRating, 1);
                provider.TotalReviews = totalReviews;

                await _unitOfWork.Clients.UpdateAsync(provider);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated provider {ProviderId} rating to {Rating} ({Count} reviews)",
                    providerId, averageRating, totalReviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating provider {ProviderId} rating", providerId);
            }
        }

        #endregion

        #region Eligibility Checks

        public async Task<ReviewEligibilityDto> CheckReviewEligibilityAsync(int clientId, int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);

            if (booking == null)
            {
                return new ReviewEligibilityDto
                {
                    CanReview = false,
                    Reason = "Booking not found."
                };
            }

            // Check if this client owns the booking
            if (booking.ClientId != clientId)
            {
                return new ReviewEligibilityDto
                {
                    CanReview = false,
                    Reason = "You can only review your own bookings."
                };
            }

            // Check if booking is completed
            if (booking.Status != BookingStatus.Completed)
            {
                return new ReviewEligibilityDto
                {
                    CanReview = false,
                    Reason = "You can only review completed bookings."
                };
            }

            // Check if already reviewed
            var existingReview = await _unitOfWork.Reviews.GetReviewByBookingIdAsync(bookingId);
            if (existingReview != null)
            {
                var canEdit = CanEditReview(existingReview);
                return new ReviewEligibilityDto
                {
                    CanReview = false,
                    Reason = "You have already reviewed this booking.",
                    HasExistingReview = true,
                    ExistingReviewId = existingReview.ReviewId,
                    CanEdit = canEdit,
                    EditDeadline = existingReview.ReviewDate.AddDays(EditWindowDays)
                };
            }

            return new ReviewEligibilityDto
            {
                CanReview = true,
                HasExistingReview = false
            };
        }

        public async Task<bool> CanClientEditReviewAsync(int clientId, int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null || review.ClientId != clientId)
                return false;

            return CanEditReview(review);
        }

        private bool CanEditReview(Review review)
        {
            return (DateTime.UtcNow - review.ReviewDate).TotalDays <= EditWindowDays;
        }

        #endregion

        #region Admin Operations

        public async Task<IEnumerable<AdminReviewDto>> GetAllReviewsForAdminAsync(bool? isVisible = null, bool? isModerated = null)
        {
            var reviews = await _unitOfWork.Reviews.GetAllReviewsForAdminAsync(isVisible, isModerated);
            return reviews.Select(r => MapToAdminDto(r));
        }

        public async Task<ReviewResultDto> ModerateReviewAsync(ModerateReviewDto dto, string adminUserId)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(dto.ReviewId);
                if (review == null)
                {
                    return new ReviewResultDto
                    {
                        Success = false,
                        Message = "Review not found."
                    };
                }

                review.IsVisible = dto.IsVisible;
                review.AdminModerated = true;

                await _unitOfWork.Reviews.UpdateAsync(review);
                await _unitOfWork.SaveChangesAsync();

                // Update provider rating if visibility changed
                await UpdateProviderRatingAsync(review.ProviderId);

                _logger.LogInformation("Review {ReviewId} moderated by admin {AdminId}. Visible: {IsVisible}",
                    dto.ReviewId, adminUserId, dto.IsVisible);

                return new ReviewResultDto
                {
                    Success = true,
                    Message = dto.IsVisible ? "Review is now visible." : "Review has been hidden.",
                    ReviewId = review.ReviewId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating review {ReviewId}", dto.ReviewId);
                return new ReviewResultDto
                {
                    Success = false,
                    Message = "An error occurred while moderating the review.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ReviewResultDto> HideReviewAsync(int reviewId, string adminUserId)
        {
            return await ModerateReviewAsync(new ModerateReviewDto
            {
                ReviewId = reviewId,
                IsVisible = false
            }, adminUserId);
        }

        public async Task<ReviewResultDto> UnhideReviewAsync(int reviewId, string adminUserId)
        {
            return await ModerateReviewAsync(new ModerateReviewDto
            {
                ReviewId = reviewId,
                IsVisible = true
            }, adminUserId);
        }

        #endregion

        #region Private Mapping Methods

        private ReviewDto MapToDto(Review review)
        {
            var clientName = review.IsAnonymous
                ? "Anonymous"
                : $"{review.Client?.FirstName} {review.Client?.LastName}";

            return new ReviewDto
            {
                ReviewId = review.ReviewId,
                BookingId = review.BookingId,
                ClientId = review.ClientId,
                ClientName = clientName,
                ClientPhotoPath = review.IsAnonymous ? null : review.Client?.ProfilePhotoPath,
                ProviderId = review.ProviderId,
                ProviderName = review.Provider != null
                    ? $"{review.Provider.FirstName} {review.Provider.LastName}"
                    : "Unknown",
                ServiceName = review.Booking?.Service?.ServiceName ?? "Unknown Service",
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate,
                IsVisible = review.IsVisible,
                IsAnonymous = review.IsAnonymous,
                AdminModerated = review.AdminModerated,
                CanEdit = CanEditReview(review)
            };
        }

        private AdminReviewDto MapToAdminDto(Review review)
        {
            return new AdminReviewDto
            {
                ReviewId = review.ReviewId,
                BookingId = review.BookingId,
                ClientName = $"{review.Client?.FirstName} {review.Client?.LastName}",
                ClientEmail = "", // You might need to fetch this from AspNetUsers if needed
                ProviderName = review.Provider != null
                    ? $"{review.Provider.FirstName} {review.Provider.LastName}"
                    : "Unknown",
                ServiceName = review.Booking?.Service?.ServiceName ?? "Unknown Service",
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate,
                IsVisible = review.IsVisible,
                IsAnonymous = review.IsAnonymous,
                AdminModerated = review.AdminModerated
            };
        }

        #endregion
    }
}