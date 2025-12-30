using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using LebAssist.Presentation.ViewModels.Provider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LebAssist.Presentation.Controllers
{
    [Authorize]
    public class ProviderController : Controller
    {
        private readonly IProviderService _providerService;
        private readonly IServiceService _serviceService;
        private readonly ICategoryService _categoryService;
        private readonly IClientService _clientService;
        private readonly IReviewService _reviewService;  // NEW
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ProviderController> _logger;

        public ProviderController(
            IProviderService providerService,
            IServiceService serviceService,
            ICategoryService categoryService,
            IClientService clientService,
            IReviewService reviewService,  // NEW
            UserManager<IdentityUser> userManager,
            ILogger<ProviderController> logger)
        {
            _providerService = providerService;
            _serviceService = serviceService;
            _categoryService = categoryService;
            _clientService = clientService;
            _reviewService = reviewService;  // NEW
            _userManager = userManager;
            _logger = logger;
        }

        // ================================
        // PUBLIC PROVIDER PROFILE (Anyone can view)
        // ================================

        /// <summary>
        /// GET: /Provider/Profile/{id}
        /// Public provider profile page - Anyone can view
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            // Get provider details
            var provider = await _clientService.GetClientByIdAsync(id);
            if (provider == null || !provider.IsProvider)
            {
                TempData["Error"] = "Provider not found.";
                return RedirectToAction("Index", "Services");
            }

            // Get provider's services
            var providerServices = await _providerService.GetProviderServicesAsync(id);

            // Get provider's portfolio
            var portfolio = await _providerService.GetProviderPortfolioAsync(id);

            // Get provider's reviews summary (show 5 recent reviews)
            var reviewsSummary = await _reviewService.GetProviderReviewsSummaryAsync(id, 1, 5);

            // Check availability status (if you have this feature)
            var isAvailable = await _providerService.IsProviderAvailableAsync(id);

            var viewModel = new ProviderProfileViewModel
            {
                ProviderId = id,
                FirstName = provider.FirstName,
                LastName = provider.LastName,
                ProfilePhotoPath = provider.ProfilePhotoPath,
                Bio = provider.Bio,
                YearsOfExperience = provider.YearsOfExperience,
                Latitude = provider.Latitude,
                Longitude = provider.Longitude,
                IsAvailable = isAvailable,
                Services = providerServices.Select(s => new ProviderServiceDto
                {
                    ProviderServiceId = s.ProviderServiceId,
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    CategoryName = s.CategoryName,
                    IsActive = s.IsActive,
                    PricePerHour = s.PricePerHour,
                    DateAdded = s.DateAdded
                }).ToList(),
                Portfolio = portfolio.Select(p => new PortfolioPhotoDto
                {
                    PortfolioPhotoId = p.PortfolioPhotoId,
                    PhotoPath = p.PhotoPath,
                    Caption = p.Caption,
                    UploadDate = p.UploadDate,
                    DisplayOrder = p.DisplayOrder
                }).ToList(),
                ReviewsSummary = reviewsSummary
            };

            return View(viewModel);
        }

        // ================================
        // APPLY TO BECOME PROVIDER (Any logged-in user)
        // ================================

        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null)
                return RedirectToAction("Login", "Account");

            // Check if already a provider or has pending application
            if (profile.IsProvider)
            {
                TempData["Info"] = "You are already a provider!";
                return RedirectToAction(nameof(MyServices));
            }

            // Get available services
            var categories = await _categoryService.GetActiveCategoriesAsync();
            var allServices = await _serviceService.GetActiveServicesAsync();

            var model = new ProviderApplicationViewModel
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                PhoneNumber = profile.PhoneNumber ?? "",
                Bio = profile.Bio ?? "",
                YearsOfExperience = profile.YearsOfExperience ?? 0,
                ServiceCategories = categories.Select(c => new ServiceCategoryGroup
                {
                    CategoryName = c.CategoryName,
                    Services = allServices
                        .Where(s => s.CategoryId == c.CategoryId)
                        .Select(s => new ServiceSelectItem
                        {
                            ServiceId = s.ServiceId,
                            ServiceName = s.ServiceName
                        }).ToList()
                }).Where(g => g.Services.Any()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ProviderApplicationViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                // Reload services
                var categories = await _categoryService.GetActiveCategoriesAsync();
                var allServices = await _serviceService.GetActiveServicesAsync();
                model.ServiceCategories = categories.Select(c => new ServiceCategoryGroup
                {
                    CategoryName = c.CategoryName,
                    Services = allServices
                        .Where(s => s.CategoryId == c.CategoryId)
                        .Select(s => new ServiceSelectItem
                        {
                            ServiceId = s.ServiceId,
                            ServiceName = s.ServiceName
                        }).ToList()
                }).Where(g => g.Services.Any()).ToList();

                return View(model);
            }

            // Check if at least one service is selected
            if (model.SelectedServiceIds == null || !model.SelectedServiceIds.Any())
            {
                ModelState.AddModelError("", "Please select at least one service you want to offer.");

                var categories = await _categoryService.GetActiveCategoriesAsync();
                var allServices = await _serviceService.GetActiveServicesAsync();
                model.ServiceCategories = categories.Select(c => new ServiceCategoryGroup
                {
                    CategoryName = c.CategoryName,
                    Services = allServices
                        .Where(s => s.CategoryId == c.CategoryId)
                        .Select(s => new ServiceSelectItem
                        {
                            ServiceId = s.ServiceId,
                            ServiceName = s.ServiceName
                        }).ToList()
                }).Where(g => g.Services.Any()).ToList();

                return View(model);
            }

            var dto = new ProviderApplicationDto
            {
                Bio = model.Bio,
                YearsOfExperience = model.YearsOfExperience,
                SelectedServiceIds = model.SelectedServiceIds
            };

            var result = await _providerService.ApplyAsProviderAsync(userId, dto);

            if (result)
            {
                TempData["Success"] = "Your provider application has been submitted! Please wait for admin approval.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Error"] = "Failed to submit application. Please try again.";
                return RedirectToAction(nameof(Apply));
            }
        }

        // Check application status
        [HttpGet]
        public async Task<IActionResult> ApplicationStatus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null)
                return RedirectToAction("Login", "Account");

            return View(profile);
        }

        // ================================
        // PROVIDER-ONLY ACTIONS
        // ================================

        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Dashboard()
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("AccessDenied", "Account");

            var profile = await _clientService.GetProfileAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var services = await _providerService.GetProviderServicesAsync(clientId.Value);
            var reviewsSummary = await _reviewService.GetProviderReviewsSummaryAsync(clientId.Value, 1, 5);

            var model = new ProviderDashboardViewModel
            {
                ProviderName = $"{profile?.FirstName} {profile?.LastName}",
                TotalServices = services.Count(),
                AverageRating = reviewsSummary.AverageRating,
                TotalReviews = reviewsSummary.TotalReviews,
                RecentReviews = reviewsSummary.Reviews.Take(3).ToList()
            };

            return View(model);
        }

        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> MyServices()
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("AccessDenied", "Account");

            var services = await _providerService.GetProviderServicesAsync(clientId.Value);

            var model = services.Select(s => new ProviderServiceViewModel
            {
                ProviderServiceId = s.ProviderServiceId,
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                CategoryName = s.CategoryName,
                IsActive = s.IsActive,
                PricePerHour = s.PricePerHour,
                DateAdded = s.DateAdded
            });

            return View(model);
        }

        [Authorize(Roles = "Provider")]
        [HttpGet]
        public async Task<IActionResult> AddService()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            var allServices = await _serviceService.GetActiveServicesAsync();

            var serviceGroups = categories.Select(c => new ServiceCategoryGroup
            {
                CategoryName = c.CategoryName,
                Services = allServices
                    .Where(s => s.CategoryId == c.CategoryId)
                    .Select(s => new ServiceSelectItem
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.ServiceName
                    }).ToList()
            }).Where(g => g.Services.Any()).ToList();

            var model = new AddProviderServiceViewModel
            {
                ServiceCategories = serviceGroups
            };

            return View(model);
        }

        [Authorize(Roles = "Provider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(AddProviderServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetActiveCategoriesAsync();
                var allServices = await _serviceService.GetActiveServicesAsync();

                model.ServiceCategories = categories.Select(c => new ServiceCategoryGroup
                {
                    CategoryName = c.CategoryName,
                    Services = allServices
                        .Where(s => s.CategoryId == c.CategoryId)
                        .Select(s => new ServiceSelectItem
                        {
                            ServiceId = s.ServiceId,
                            ServiceName = s.ServiceName
                        }).ToList()
                }).Where(g => g.Services.Any()).ToList();

                return View(model);
            }

            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("AccessDenied", "Account");

            var dto = new AddProviderServiceDto
            {
                ServiceId = model.ServiceId,
                PricePerHour = model.PricePerHour
            };

            var result = await _providerService.AddProviderServiceAsync(clientId.Value, dto);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to add service. You may already offer this service.");

                var categories = await _categoryService.GetActiveCategoriesAsync();
                var allServices = await _serviceService.GetActiveServicesAsync();

                model.ServiceCategories = categories.Select(c => new ServiceCategoryGroup
                {
                    CategoryName = c.CategoryName,
                    Services = allServices
                        .Where(s => s.CategoryId == c.CategoryId)
                        .Select(s => new ServiceSelectItem
                        {
                            ServiceId = s.ServiceId,
                            ServiceName = s.ServiceName
                        }).ToList()
                }).Where(g => g.Services.Any()).ToList();

                return View(model);
            }

            TempData["SuccessMessage"] = "Service added successfully!";
            return RedirectToAction(nameof(MyServices));
        }

        [Authorize(Roles = "Provider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveService(int serviceId)
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return Json(new { success = false, message = "User not authenticated" });

            var result = await _providerService.RemoveProviderServiceAsync(clientId.Value, serviceId);

            if (!result)
                return Json(new { success = false, message = "Cannot remove service. You may have active bookings." });

            return Json(new { success = true });
        }

        [Authorize(Roles = "Provider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateServicePrice(int serviceId, decimal pricePerHour)
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return Json(new { success = false, message = "User not authenticated" });

            var result = await _providerService.UpdateProviderServicePriceAsync(clientId.Value, serviceId, pricePerHour);

            return Json(new { success = result });
        }

        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Portfolio()
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("AccessDenied", "Account");

            var photos = await _providerService.GetProviderPortfolioAsync(clientId.Value);

            var model = new PortfolioViewModel
            {
                Photos = photos.Select(p => new PortfolioPhotoViewModel
                {
                    PortfolioPhotoId = p.PortfolioPhotoId,
                    PhotoPath = p.PhotoPath,
                    Caption = p.Caption,
                    UploadDate = p.UploadDate,
                    DisplayOrder = p.DisplayOrder
                }).ToList()
            };

            return View(model);
        }

        [Authorize(Roles = "Provider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPortfolioPhoto(AddPortfolioPhotoViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Portfolio));

            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("AccessDenied", "Account");

            using var memoryStream = new MemoryStream();
            await model.Photo.CopyToAsync(memoryStream);
            var photoData = memoryStream.ToArray();

            var photoId = await _providerService.AddPortfolioPhotoAsync(
                clientId.Value,
                photoData,
                model.Photo.FileName,
                model.Caption);

            if (photoId == 0)
            {
                TempData["ErrorMessage"] = "Failed to upload photo. Please ensure it's a valid image file (jpg, png, webp) and under 5MB.";
            }
            else
            {
                TempData["SuccessMessage"] = "Photo added successfully!";
            }

            return RedirectToAction(nameof(Portfolio));
        }

        [Authorize(Roles = "Provider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePortfolioPhoto(int photoId)
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return Json(new { success = false, message = "User not authenticated" });

            var result = await _providerService.DeletePortfolioPhotoAsync(clientId.Value, photoId);

            return Json(new { success = result });
        }

        [Authorize(Roles = "Provider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderPortfolio([FromBody] List<int> photoIds)
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return Json(new { success = false, message = "User not authenticated" });

            var result = await _providerService.ReorderPortfolioPhotosAsync(clientId.Value, photoIds);

            return Json(new { success = result });
        }

        // ================================
        // PROVIDER'S OWN REVIEWS
        // ================================

        /// <summary>
        /// GET: /Provider/MyReviews
        /// Provider views reviews they received
        /// </summary>
        [Authorize(Roles = "Provider")]
        [HttpGet]
        public async Task<IActionResult> MyReviews(int page = 1)
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
                return RedirectToAction("AccessDenied", "Account");

            var reviewsSummary = await _reviewService.GetProviderReviewsSummaryAsync(clientId.Value, page, 10);

            return View(reviewsSummary);
        }

        private async Task<int?> GetCurrentClientIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return null;

            var profile = await _clientService.GetProfileAsync(userId);
            return profile?.ClientId;
        }
    }
}