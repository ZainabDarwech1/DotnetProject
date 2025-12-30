using Domain.Enums;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using LebAssist.Presentation.Hubs;
using LebAssist.Presentation.ViewModels.Emergency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LebAssist.Presentation.Controllers
{
    [Authorize]
    public class EmergencyController : Controller
    {
        private readonly IEmergencyService _emergencyService;
        private readonly IClientService _clientService;
        private readonly IServiceService _serviceService;
        private readonly IHubContext<EmergencyHub> _emergencyHub;

        public EmergencyController(
            IEmergencyService emergencyService,
            IClientService clientService,
            IServiceService serviceService,
            IHubContext<EmergencyHub> emergencyHub)
        {
            _emergencyService = emergencyService ?? throw new ArgumentNullException(nameof(emergencyService));
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _serviceService = serviceService ?? throw new ArgumentNullException(nameof(serviceService));
            _emergencyHub = emergencyHub ?? throw new ArgumentNullException(nameof(emergencyHub));
        }

        // GET: /Emergency
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            var emergencies = await _emergencyService.GetClientEmergenciesAsync(profile.ClientId);
            return View(emergencies);
        }

        // GET: /Emergency/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var services = await _serviceService.GetAllServicesAsync();

            var model = new CreateEmergencyViewModel
            {
                Services = services.Select(s => new EmergencyServiceSelectItem
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    CategoryName = s.CategoryName
                }).ToList(),
                Latitude = 33.8938,
                Longitude = 35.5018
            };

            return View(model);
        }

        // POST: /Emergency/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmergencyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var services = await _serviceService.GetAllServicesAsync();
                model.Services = services.Select(s => new EmergencyServiceSelectItem
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    CategoryName = s.CategoryName
                }).ToList();
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            var dto = new EmergencyDtos
            {
                ServiceId = model.ServiceId,
                Description = model.Description ?? string.Empty,
                LocationAddress = model.LocationAddress ?? string.Empty,
                Latitude = model.Latitude,
                Longitude = model.Longitude
            };

            if (model.Photo != null && model.Photo.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await model.Photo.CopyToAsync(memoryStream);
                dto.PhotoData = memoryStream.ToArray();
                dto.PhotoFileName = model.Photo.FileName;
            }

            var emergencyId = await _emergencyService.CreateEmergencyAsync(profile.ClientId, dto);

            // Get service name for notification
            var allServices = await _serviceService.GetAllServicesAsync();
            var serviceName = allServices.FirstOrDefault(s => s.ServiceId == model.ServiceId)?.ServiceName ?? "Service";

            // Broadcast to all providers via SignalR
            await _emergencyHub.Clients.Group("Providers").SendAsync("OnEmergencyReceived", new
            {
                emergencyRequestId = emergencyId,
                description = model.Description ?? string.Empty,
                latitude = model.Latitude,
                longitude = model.Longitude,
                serviceName = serviceName,
                requesterId = profile.ClientId
            });

            TempData["Success"] = "Emergency request sent to all available providers!";
            return RedirectToAction("Details", new { id = emergencyId });
        }

        // GET: /Emergency/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var emergency = await _emergencyService.GetEmergencyDetailsAsync(id);
            if (emergency == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            var model = new EmergencyDetailsViewModel
            {
                EmergencyRequestId = emergency.EmergencyRequestId,
                ClientName = $"{emergency.Client?.FirstName ?? ""} {emergency.Client?.LastName ?? ""}".Trim(),
                ClientPhone = emergency.Client?.PhoneNumber ?? "",
                ServiceName = emergency.Service?.ServiceName ?? "Unknown Service",
                ProviderName = emergency.Provider != null ? $"{emergency.Provider.FirstName} {emergency.Provider.LastName}" : null,
                ProviderPhone = emergency.Provider?.PhoneNumber,
                Details = emergency.Details ?? "",
                Latitude = (double)emergency.Latitude,
                Longitude = (double)emergency.Longitude,
                Status = emergency.Status.ToString(),
                RequestDateTime = emergency.RequestDateTime,
                AcceptedDateTime = emergency.AcceptedDateTime,
                CompletedDate = emergency.CompletedDate,
                IsOwner = emergency.ClientId == profile.ClientId,
                IsProvider = User.IsInRole("Provider"),
                IsAssignedProvider = emergency.ProviderId == profile.ClientId
            };

            return View(model);
        }

        // GET: /Emergency/Pending
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Pending()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            var emergencies = await _emergencyService.GetPendingEmergenciesAsync();

            var filteredEmergencies = emergencies.Where(e => e.ClientId != profile.ClientId);

            var model = filteredEmergencies.Select(e => new EmergencyListItemViewModel
            {
                EmergencyRequestId = e.EmergencyRequestId,
                ServiceName = e.Service?.ServiceName ?? "Unknown",
                Details = e.Details ?? "",
                Status = e.Status.ToString(),
                RequestDateTime = e.RequestDateTime,
                Latitude = (double)e.Latitude,
                Longitude = (double)e.Longitude,
                Distance = CalculateDistance(
                    (double)profile.Latitude,
                    (double)profile.Longitude,
                    (double)e.Latitude,
                    (double)e.Longitude)
            }).OrderBy(e => e.Distance).ToList();

            return View(model);
        }

        // POST: /Emergency/Accept
        [Authorize(Roles = "Provider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int emergencyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            var emergency = await _emergencyService.GetEmergencyDetailsAsync(emergencyId);
            if (emergency != null && emergency.ClientId == profile.ClientId)
            {
                TempData["Error"] = "You cannot accept your own emergency request.";
                return RedirectToAction("Pending");
            }

            var result = await _emergencyService.AcceptEmergencyAsync(emergencyId, profile.ClientId);

            if (result)
            {
                await _emergencyHub.Clients.Group("Providers").SendAsync("OnEmergencyRemoved", emergencyId);

                TempData["Success"] = "Emergency accepted! Contact the client.";
                return RedirectToAction("Details", new { id = emergencyId });
            }
            else
            {
                TempData["Error"] = "This emergency has already been accepted by another provider.";
                return RedirectToAction("Pending");
            }
        }

        // POST: /Emergency/Decline
        [Authorize(Roles = "Provider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(int emergencyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _emergencyService.DeclineEmergencyAsync(emergencyId, profile.ClientId);

            TempData["Info"] = "Emergency declined.";
            return RedirectToAction("Pending");
        }

        // POST: /Emergency/Start
        [Authorize(Roles = "Provider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int emergencyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _emergencyService.StartEmergencyAsync(emergencyId, profile.ClientId);

            TempData["Success"] = "Emergency marked as In Progress.";
            return RedirectToAction("Details", new { id = emergencyId });
        }

        // POST: /Emergency/Complete
        [Authorize(Roles = "Provider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int emergencyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _emergencyService.CompleteEmergencyAsync(emergencyId, profile.ClientId);

            TempData["Success"] = "Emergency completed successfully!";
            return RedirectToAction("Details", new { id = emergencyId });
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double degrees) => degrees * Math.PI / 180;
    }
}