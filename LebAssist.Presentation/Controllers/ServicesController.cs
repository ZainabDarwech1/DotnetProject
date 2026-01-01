using LebAssist.Application.Interfaces;
using LebAssist.Presentation.ViewModels.Services;
using Microsoft.AspNetCore.Mvc;

namespace LebAssist.Presentation.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IServiceService _serviceService;
        private readonly IProviderService _providerService;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(
            ICategoryService categoryService,
            IServiceService serviceService,
            IProviderService providerService,
            ILogger<ServicesController> logger)
        {
            _categoryService = categoryService;
            _serviceService = serviceService; // will fix
            _providerService = providerService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return View(categories);
        }

        public async Task<IActionResult> Category(int id)
        {
            var category = await _categoryService.GetCategoryWithServicesAsync(id);

            if (category == null || !category.IsActive)
                return NotFound();

            return View(category);
        }

        public async Task<IActionResult> Details(int id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id);

            if (service == null || !service.IsActive)
                return NotFound();

            return View(service);
        }

        public async Task<IActionResult> Providers(int serviceId)
        {
            var service = await _serviceService.GetServiceByIdAsync(serviceId);
            if (service == null) return NotFound();

            var providers = await _providerService.GetProvidersByServiceIdAsync(serviceId);

            var model = new ServiceProvidersViewModel
            {
                Service = service,
                Providers = providers.Select(p => new ProviderOfferingViewModel
                {
                    ProviderId = p.ProviderId,
                    ProviderName = p.ProviderName,
                    ProviderPhotoPath = p.ProviderPhotoPath,
                    ProviderServiceId = p.ProviderServiceId,
                    PricePerHour = p.PricePerHour
                }).ToList()
            };

            return View(model);
        }
    }
}
