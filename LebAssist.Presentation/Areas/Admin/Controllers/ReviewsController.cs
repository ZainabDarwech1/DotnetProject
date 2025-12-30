using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using LebAssist.Presentation.ViewModels.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LebAssist.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(
            IReviewService reviewService,
            ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// GET: /Admin/Reviews
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? visibility = null, string? moderated = null)
        {
            bool? isVisible = visibility switch
            {
                "visible" => true,
                "hidden" => false,
                _ => null
            };

            bool? isModerated = moderated switch
            {
                "moderated" => true,
                "unmoderated" => false,
                _ => null
            };

            var reviews = await _reviewService.GetAllReviewsForAdminAsync(isVisible, isModerated);
            var reviewsList = reviews.ToList();

            var viewModel = new AdminReviewListViewModel
            {
                Reviews = reviewsList,
                FilterVisibility = visibility,
                FilterModerated = moderated,
                TotalReviews = reviewsList.Count,
                VisibleCount = reviewsList.Count(r => r.IsVisible),
                HiddenCount = reviewsList.Count(r => !r.IsVisible),
                ModeratedCount = reviewsList.Count(r => r.AdminModerated)
            };

            return View(viewModel);
        }

        /// <summary>
        /// GET: /Admin/Reviews/Details/{id}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
            {
                TempData["Error"] = "Review not found.";
                return RedirectToAction(nameof(Index));
            }

            var adminReview = new AdminReviewDto
            {
                ReviewId = review.ReviewId,
                BookingId = review.BookingId,
                ClientName = review.ClientName,
                ProviderName = review.ProviderName,
                ServiceName = review.ServiceName,
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate,
                IsVisible = review.IsVisible,
                IsAnonymous = review.IsAnonymous,
                AdminModerated = review.AdminModerated
            };

            var viewModel = new AdminReviewDetailViewModel
            {
                Review = adminReview
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST: /Admin/Reviews/Hide/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
            var result = await _reviewService.HideReviewAsync(id, adminUserId);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// POST: /Admin/Reviews/Unhide/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unhide(int id)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
            var result = await _reviewService.UnhideReviewAsync(id, adminUserId);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// POST: /Admin/Reviews/Delete/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";

            // Pass 0 as clientId since admin is deleting
            var result = await _reviewService.DeleteReviewAsync(id, 0, isAdmin: true);

            if (result.Success)
            {
                TempData["Success"] = "Review deleted successfully.";
                _logger.LogWarning("Admin {AdminId} deleted review {ReviewId}", adminUserId, id);
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}