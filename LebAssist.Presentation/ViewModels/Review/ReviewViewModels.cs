using LebAssist.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace LebAssist.Presentation.ViewModels.Review
{
    /// <summary>
    /// ViewModel for submitting a new review
    /// </summary>
    public class SubmitReviewViewModel
    {
        public int BookingId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Please select a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }

        public bool IsAnonymous { get; set; } = false;
    }

    /// <summary>
    /// ViewModel for editing an existing review
    /// </summary>
    public class EditReviewViewModel
    {
        public int ReviewId { get; set; }
        public int BookingId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime ReviewDate { get; set; }
        public DateTime EditDeadline { get; set; }
        public int DaysRemaining { get; set; }

        [Required(ErrorMessage = "Please select a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }

        public bool IsAnonymous { get; set; }
    }

    /// <summary>
    /// ViewModel for displaying provider reviews
    /// </summary>
    public class ProviderReviewsViewModel
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string? ProviderPhotoPath { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public Dictionary<int, double> RatingPercentages { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public string SortBy { get; set; } = "newest";
    }

    /// <summary>
    /// ViewModel for client's own reviews list
    /// </summary>
    public class MyReviewsViewModel
    {
        public List<ReviewDto> Reviews { get; set; } = new();
        public int TotalReviews { get; set; }
    }

    /// <summary>
    /// ViewModel for admin review management list
    /// </summary>
    public class AdminReviewListViewModel
    {
        public List<AdminReviewDto> Reviews { get; set; } = new();
        public string? FilterVisibility { get; set; }
        public string? FilterModerated { get; set; }
        public int TotalReviews { get; set; }
        public int VisibleCount { get; set; }
        public int HiddenCount { get; set; }
        public int ModeratedCount { get; set; }
    }

    /// <summary>
    /// ViewModel for admin review detail/moderation
    /// </summary>
    public class AdminReviewDetailViewModel
    {
        public AdminReviewDto Review { get; set; } = new();
        public string? AdminNotes { get; set; }
    }
}