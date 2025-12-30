using LebAssist.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LebAssist.Presentation.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IServiceService _serviceService;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(
            ICategoryService categoryService,
            IServiceService serviceService,
            ILogger<ServicesController> logger)
        {
            _categoryService = categoryService;
            _serviceService = serviceService;
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
    }
}
