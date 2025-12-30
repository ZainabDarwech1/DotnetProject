using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using LebAssist.Presentation.ViewModels.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LebAssist.Presentation.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IBookingService _bookingService;
        private readonly IClientService _clientService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(
            IReviewService reviewService,
            IBookingService bookingService,
            IClientService clientService,
            ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _bookingService = bookingService;
            _clientService = clientService;
            _logger = logger;
        }

        #region Helper Methods

        private async Task<int?> GetCurrentClientIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;

            var client = await _clientService.GetClientByAspNetUserIdAsync(userId);
            return client?.ClientId;
        }

        #endregion

        #region Submit Review

        /// <summary>
        /// GET: /Review/Create/{bookingId}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(int bookingId)
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("Login", "Account");

            // Check eligibility
            var eligibility = await _reviewService.CheckReviewEligibilityAsync(clientId.Value, bookingId);

            if (!eligibility.CanReview)
            {
                if (eligibility.HasExistingReview && eligibility.CanEdit)
                {
                    return RedirectToAction(nameof(Edit), new { id = eligibility.ExistingReviewId });
                }

                TempData["Error"] = eligibility.Reason;
                return RedirectToAction("Details", "BookingDetails", new { id = bookingId });
            }

            // Get booking details for display
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("MyBookings", "Booking");
            }

            var viewModel = new SubmitReviewViewModel
            {
                BookingId = bookingId,
                ProviderName = booking.ProviderName,
                ServiceName = booking.ServiceName,
                BookingDate = booking.ScheduledDateTime
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST: /Review/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubmitReviewViewModel model)
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                // Reload booking details
                var booking = await _bookingService.GetBookingByIdAsync(model.BookingId);
                if (booking != null)
                {
                    model.ProviderName = booking.ProviderName;
                    model.ServiceName = booking.ServiceName;
                    model.BookingDate = booking.ScheduledDateTime;
                }
                return View(model);
            }

            var dto = new CreateReviewDto
            {
                BookingId = model.BookingId,
                Rating = model.Rating,
                Comment = model.Comment,
                IsAnonymous = model.IsAnonymous
            };

            var result = await _reviewService.CreateReviewAsync(clientId.Value, dto);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction("Details", "BookingDetails", new { id = model.BookingId });
            }

            TempData["Error"] = result.Message;

            // Reload booking details
            var bookingDetails = await _bookingService.GetBookingByIdAsync(model.BookingId);
            if (bookingDetails != null)
            {
                model.ProviderName = bookingDetails.ProviderName;
                model.ServiceName = bookingDetails.ServiceName;
                model.BookingDate = bookingDetails.ScheduledDateTime;
            }

            return View(model);
        }

        #endregion

        #region Edit Review

        /// <summary>
        /// GET: /Review/Edit/{id}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("Login", "Account");

            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
            {
                TempData["Error"] = "Review not found.";
                return RedirectToAction(nameof(MyReviews));
            }

            // Verify ownership
            if (review.ClientId != clientId.Value)
            {
                TempData["Error"] = "You can only edit your own reviews.";
                return RedirectToAction(nameof(MyReviews));
            }

            // Check if can still edit
            if (!review.CanEdit)
            {
                TempData["Error"] = "This review can no longer be edited.";
                return RedirectToAction(nameof(MyReviews));
            }

            var viewModel = new EditReviewViewModel
            {
                ReviewId = review.ReviewId,
                BookingId = review.BookingId,
                ProviderName = review.ProviderName,
                ServiceName = review.ServiceName,
                ReviewDate = review.ReviewDate,
                EditDeadline = review.ReviewDate.AddDays(7),
                DaysRemaining = Math.Max(0, 7 - (int)(DateTime.UtcNow - review.ReviewDate).TotalDays),
                Rating = review.Rating,
                Comment = review.Comment,
                IsAnonymous = review.IsAnonymous
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST: /Review/Edit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditReviewViewModel model)
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = new UpdateReviewDto
            {
                ReviewId = model.ReviewId,
                Rating = model.Rating,
                Comment = model.Comment,
                IsAnonymous = model.IsAnonymous
            };

            var result = await _reviewService.UpdateReviewAsync(clientId.Value, dto);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction("Details", "BookingDetails", new { id = model.BookingId });
            }

            TempData["Error"] = result.Message;
            return View(model);
        }

        #endregion

        #region Delete Review

        /// <summary>
        /// POST: /Review/Delete/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl = null)
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("Login", "Account");

            var result = await _reviewService.DeleteReviewAsync(id, clientId.Value);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(MyReviews));
        }

        #endregion

        #region My Reviews

        /// <summary>
        /// GET: /Review/MyReviews
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyReviews()
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("Login", "Account");

            var reviews = await _reviewService.GetClientReviewsAsync(clientId.Value);

            var viewModel = new MyReviewsViewModel
            {
                Reviews = reviews.ToList(),
                TotalReviews = reviews.Count()
            };

            return View(viewModel);
        }

        #endregion

        #region Provider Reviews (Public)

        /// <summary>
        /// GET: /Review/ProviderReviews/{providerId}
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ProviderReviews(int providerId, int page = 1, string sortBy = "newest")
        {
            var summary = await _reviewService.GetProviderReviewsSummaryAsync(providerId, page, 10);

            // Sort reviews based on sortBy parameter
            var sortedReviews = sortBy switch
            {
                "highest" => summary.Reviews.OrderByDescending(r => r.Rating).ThenByDescending(r => r.ReviewDate).ToList(),
                "lowest" => summary.Reviews.OrderBy(r => r.Rating).ThenByDescending(r => r.ReviewDate).ToList(),
                _ => summary.Reviews // Default is newest first (already sorted by API)
            };

            var client = await _clientService.GetClientByIdAsync(providerId);

            var viewModel = new ProviderReviewsViewModel
            {
                ProviderId = providerId,
                ProviderName = summary.ProviderName,
                ProviderPhotoPath = client?.ProfilePhotoPath,
                AverageRating = summary.AverageRating,
                TotalReviews = summary.TotalReviews,
                RatingDistribution = summary.RatingDistribution,
                RatingPercentages = summary.RatingPercentages,
                Reviews = sortedReviews,
                CurrentPage = page,
                TotalPages = summary.TotalPages,
                PageSize = summary.PageSize,
                SortBy = sortBy
            };

            return View(viewModel);
        }

        #endregion

        #region AJAX Endpoints

        /// <summary>
        /// GET: /Review/CheckEligibility/{bookingId}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckEligibility(int bookingId)
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return Json(new { canReview = false, reason = "Not authenticated" });

            var eligibility = await _reviewService.CheckReviewEligibilityAsync(clientId.Value, bookingId);
            return Json(eligibility);
        }

        /// <summary>
        /// GET: /Review/LoadMoreReviews
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> LoadMoreReviews(int providerId, int page = 1, string sortBy = "newest")
        {
            var summary = await _reviewService.GetProviderReviewsSummaryAsync(providerId, page, 10);

            var sortedReviews = sortBy switch
            {
                "highest" => summary.Reviews.OrderByDescending(r => r.Rating).ToList(),
                "lowest" => summary.Reviews.OrderBy(r => r.Rating).ToList(),
                _ => summary.Reviews
            };

            return PartialView("_ReviewsList", sortedReviews);
        }

        #endregion
    }
}