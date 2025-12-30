using System.ComponentModel.DataAnnotations;

namespace LebAssist.Application.DTOs
{
    /// <summary>
    /// DTO for displaying a review
    /// </summary>
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int BookingId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPhotoPath { get; set; }
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsVisible { get; set; }
        public bool IsAnonymous { get; set; }
        public bool AdminModerated { get; set; }
        public bool CanEdit { get; set; }
        public string TimeAgo => GetTimeAgo(ReviewDate);

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w ago";
            return dateTime.ToString("MMM dd, yyyy");
        }
    }

    /// <summary>
    /// DTO for creating a new review
    /// </summary>
    public class CreateReviewDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }

        public bool IsAnonymous { get; set; } = false;
    }

    /// <summary>
    /// DTO for editing an existing review
    /// </summary>
    public class UpdateReviewDto
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }

        public bool IsAnonymous { get; set; }
    }

    /// <summary>
    /// DTO for provider reviews summary
    /// </summary>
    public class ProviderReviewsSummaryDto
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public Dictionary<int, double> RatingPercentages { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// DTO for admin review management
    /// </summary>
    public class AdminReviewDto
    {
        public int ReviewId { get; set; }
        public int BookingId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsVisible { get; set; }
        public bool IsAnonymous { get; set; }
        public bool AdminModerated { get; set; }
    }

    /// <summary>
    /// DTO for admin moderation action
    /// </summary>
    public class ModerateReviewDto
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        public bool IsVisible { get; set; }

        public string? AdminNotes { get; set; }
    }

    /// <summary>
    /// Result DTO for review operations
    /// </summary>
    public class ReviewResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ReviewId { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// DTO for review eligibility check
    /// </summary>
    public class ReviewEligibilityDto
    {
        public bool CanReview { get; set; }
        public string? Reason { get; set; }
        public int? ExistingReviewId { get; set; }
        public bool HasExistingReview { get; set; }
        public bool CanEdit { get; set; }
        public DateTime? EditDeadline { get; set; }
    }
}