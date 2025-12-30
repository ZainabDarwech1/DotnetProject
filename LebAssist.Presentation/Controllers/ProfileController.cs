using LebAssist.Application.Interfaces;
using LebAssist.Presentation.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LebAssist.Presentation.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IClientService clientService,
            ILogger<ProfileController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null)
                return NotFound();

            var model = new ProfileViewModel
            {
                ClientId = profile.ClientId,
                Email = profile.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                PhoneNumber = profile.PhoneNumber,
                Latitude = profile.Latitude,
                Longitude = profile.Longitude,
                ProfilePhotoPath = profile.ProfilePhotoPath,
                IsProvider = profile.IsProvider,
                Bio = profile.Bio,
                YearsOfExperience = profile.YearsOfExperience
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null)
                return NotFound();

            var model = new EditProfileViewModel
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                PhoneNumber = profile.PhoneNumber,
                Latitude = profile.Latitude,
                Longitude = profile.Longitude,
                Bio = profile.Bio,
                YearsOfExperience = profile.YearsOfExperience
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var dto = new LebAssist.Application.DTOs.UpdateClientProfileDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Bio = model.Bio,
                YearsOfExperience = model.YearsOfExperience
            };

            var result = await _clientService.UpdateProfileAsync(userId, dto);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to update profile.");
                return View(model);
            }

            if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await model.ProfilePhoto.CopyToAsync(memoryStream);
                var photoData = memoryStream.ToArray();

                await _clientService.UpdateProfilePhotoAsync(userId, photoData, model.ProfilePhoto.FileName);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var result = await _clientService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to change password. Please verify your current password.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfilePhoto()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Json(new { success = false, message = "User not authenticated" });

            var result = await _clientService.DeleteProfilePhotoAsync(userId);

            return Json(new { success = result });
        }
        [HttpGet]
        public IActionResult TestUpload()
        {
            return View();
        }
    }
}
