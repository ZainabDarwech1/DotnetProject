using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using LebAssist.Presentation.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LebAssist.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceController : Controller
    {
        private readonly IServiceService _serviceService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(
            IServiceService serviceService,
            ICategoryService categoryService,
            ILogger<ServiceController> logger)
        {
            _serviceService = serviceService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            IEnumerable<ServiceDto> services;

            if (categoryId.HasValue)
            {
                services = await _serviceService.GetServicesByCategoryAsync(categoryId.Value);
            }
            else
            {
                services = await _serviceService.GetAllServicesAsync();
            }

            var model = services.Select(s => new ServiceViewModel
            {
                ServiceId = s.ServiceId,
                CategoryId = s.CategoryId,
                CategoryName = s.CategoryName,
                ServiceName = s.ServiceName,
                ServiceDescription = s.ServiceDescription,
                IsActive = s.IsActive,
                ServiceIconPath = s.ServiceIconPath
            });

            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();

            var model = new CreateServiceViewModel
            {
                Categories = categories.Select(c => new CategorySelectItem
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetActiveCategoriesAsync();
                model.Categories = categories.Select(c => new CategorySelectItem
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                }).ToList();

                return View(model);
            }

            try
            {
                byte[]? iconData = null;
                string? iconFileName = null;

                if (model.ServiceIcon != null && model.ServiceIcon.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await model.ServiceIcon.CopyToAsync(memoryStream);
                    iconData = memoryStream.ToArray();
                    iconFileName = model.ServiceIcon.FileName;
                }

                var dto = new CreateServiceDto
                {
                    CategoryId = model.CategoryId,
                    ServiceName = model.ServiceName,
                    ServiceDescription = model.ServiceDescription,
                    ServiceIconData = iconData,
                    ServiceIconFileName = iconFileName
                };

                var serviceId = await _serviceService.CreateServiceAsync(dto);

                TempData["SuccessMessage"] = "Service created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the service.");

                var categories = await _categoryService.GetActiveCategoriesAsync();
                model.Categories = categories.Select(c => new CategorySelectItem
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                }).ToList();

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
                return NotFound();

            var categories = await _categoryService.GetAllCategoriesAsync();

            var model = new EditServiceViewModel
            {
                ServiceId = service.ServiceId,
                CategoryId = service.CategoryId,
                ServiceName = service.ServiceName,
                ServiceDescription = service.ServiceDescription,
                CurrentIconPath = service.ServiceIconPath,
                IsActive = service.IsActive,
                Categories = categories.Select(c => new CategorySelectItem
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                model.Categories = categories.Select(c => new CategorySelectItem
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                }).ToList();

                return View(model);
            }

            try
            {
                byte[]? iconData = null;
                string? iconFileName = null;

                if (model.ServiceIcon != null && model.ServiceIcon.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await model.ServiceIcon.CopyToAsync(memoryStream);
                    iconData = memoryStream.ToArray();
                    iconFileName = model.ServiceIcon.FileName;
                }

                var dto = new UpdateServiceDto
                {
                    CategoryId = model.CategoryId,
                    ServiceName = model.ServiceName,
                    ServiceDescription = model.ServiceDescription,
                    ServiceIconData = iconData,
                    ServiceIconFileName = iconFileName
                };

                var result = await _serviceService.UpdateServiceAsync(model.ServiceId, dto);

                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "Failed to update service.");

                    var categories = await _categoryService.GetAllCategoriesAsync();
                    model.Categories = categories.Select(c => new CategorySelectItem
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName
                    }).ToList();

                    return View(model);
                }

                TempData["SuccessMessage"] = "Service updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the service.");

                var categories = await _categoryService.GetAllCategoriesAsync();
                model.Categories = categories.Select(c => new CategorySelectItem
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                }).ToList();

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _serviceService.ToggleServiceStatusAsync(id);

            if (!result)
                return Json(new { success = false, message = "Failed to toggle service status." });

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _serviceService.DeleteServiceAsync(id);

            if (!result)
            {
                TempData["ErrorMessage"] = "Cannot delete service. It may be in use by providers or bookings.";
            }
            else
            {
                TempData["SuccessMessage"] = "Service deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
