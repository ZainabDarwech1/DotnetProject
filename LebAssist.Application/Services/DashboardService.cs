using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;

namespace LebAssist.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var allClients = await _unitOfWork.Clients.GetAllAsync();
            var allBookings = await _unitOfWork.Bookings.GetAllAsync();
            var allCategories = await _unitOfWork.Categories.GetAllAsync();
            var allServices = await _unitOfWork.Services.GetAllAsync();
            var allReviews = await _unitOfWork.Reviews.GetAllAsync();
            var allEmergencies = await _unitOfWork.EmergencyRequests.GetAllAsync();

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            return new DashboardStatsDto
            {
                TotalUsers = allClients.Count(),
                TotalProviders = allClients.Count(c => c.IsProvider && c.ProviderStatus == ProviderStatus.Approved),
                TotalClients = allClients.Count(),
                TotalBookings = allBookings.Count(),
                PendingBookings = allBookings.Count(b => b.Status == BookingStatus.Pending),
                CompletedBookings = allBookings.Count(b => b.Status == BookingStatus.Completed),
                TotalCategories = allCategories.Count(),
                TotalServices = allServices.Count(),
                TotalReviews = allReviews.Count(),
                AverageRating = allReviews.Any() ? allReviews.Average(r => r.Rating) : 0,
                TotalEmergencies = allEmergencies.Count(),
                PendingEmergencies = allEmergencies.Count(e => e.Status == EmergencyStatus.Pending),
                NewUsersThisMonth = allClients.Count(c => c.DateRegistered.Month == currentMonth && c.DateRegistered.Year == currentYear),
                BookingsThisMonth = allBookings.Count(b => b.RequestDate.Month == currentMonth && b.RequestDate.Year == currentYear),
                TotalRevenue = 0,
                RevenueThisMonth = 0
            };
        }

        public async Task<IEnumerable<BookingTrendDto>> GetBookingTrendsAsync(int months = 6)
        {
            var allBookings = await _unitOfWork.Bookings.GetAllAsync();
            var startDate = DateTime.UtcNow.AddMonths(-months);

            return allBookings
                .Where(b => b.RequestDate >= startDate)
                .GroupBy(b => new { b.RequestDate.Year, b.RequestDate.Month })
                .Select(g => new BookingTrendDto
                {
                    Year = g.Key.Year,
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    TotalBookings = g.Count(),
                    CompletedBookings = g.Count(b => b.Status == BookingStatus.Completed),
                    CancelledBookings = g.Count(b => b.Status == BookingStatus.Cancelled)
                })
                .OrderBy(t => t.Year)
                .ThenBy(t => DateTime.ParseExact(t.Month, "MMM yyyy", null).Month)
                .ToList();
        }

        public async Task<IEnumerable<CategoryStatsDto>> GetCategoryStatsAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var services = await _unitOfWork.Services.GetAllAsync();
            var bookings = await _unitOfWork.Bookings.GetAllAsync();
            var providerServices = await _unitOfWork.ProviderServices.GetAllAsync();

            return categories.Select(c => new CategoryStatsDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                ServiceCount = services.Count(s => s.CategoryId == c.CategoryId),
                BookingCount = bookings.Count(b => services.Any(s => s.ServiceId == b.ServiceId && s.CategoryId == c.CategoryId)),
                ProviderCount = providerServices.Count(ps => services.Any(s => s.ServiceId == ps.ServiceId && s.CategoryId == c.CategoryId))
            }).OrderByDescending(c => c.BookingCount).ToList();
        }

        public async Task<IEnumerable<ServicePopularityDto>> GetTopServicesAsync(int count = 10)
        {
            var services = await _unitOfWork.Services.GetAllAsync();
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var bookings = await _unitOfWork.Bookings.GetAllAsync();
            var providerServices = await _unitOfWork.ProviderServices.GetAllAsync();
            var reviews = await _unitOfWork.Reviews.GetAllAsync();

            return services
                .Select(s => new ServicePopularityDto
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    CategoryName = categories.FirstOrDefault(c => c.CategoryId == s.CategoryId)?.CategoryName ?? "Unknown",
                    BookingCount = bookings.Count(b => b.ServiceId == s.ServiceId),
                    ProviderCount = providerServices.Count(ps => ps.ServiceId == s.ServiceId),
                    AverageRating = bookings
                        .Where(b => b.ServiceId == s.ServiceId && reviews.Any(r => r.BookingId == b.BookingId))
                        .Select(b => reviews.FirstOrDefault(r => r.BookingId == b.BookingId)?.Rating ?? 0)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .OrderByDescending(s => s.BookingCount)
                .Take(count)
                .ToList();
        }

        public async Task<IEnumerable<ProviderPerformanceDto>> GetTopProvidersAsync(int count = 10)
        {
            var clients = await _unitOfWork.Clients.GetAllAsync();
            var bookings = await _unitOfWork.Bookings.GetAllAsync();
            var reviews = await _unitOfWork.Reviews.GetAllAsync();

            var providers = clients.Where(c => c.IsProvider && c.ProviderStatus == ProviderStatus.Approved);

            return providers
                .Select(p => new ProviderPerformanceDto
                {
                    ProviderId = p.ClientId,
                    ProviderName = $"{p.FirstName} {p.LastName}",
                    ProfilePhotoPath = p.ProfilePhotoPath,
                    CompletedBookings = bookings.Count(b => b.ProviderId == p.ClientId && b.Status == BookingStatus.Completed),
                    TotalReviews = reviews.Count(r => bookings.Any(b => b.BookingId == r.BookingId && b.ProviderId == p.ClientId)),
                    AverageRating = bookings
                        .Where(b => b.ProviderId == p.ClientId && reviews.Any(r => r.BookingId == b.BookingId))
                        .Select(b => reviews.FirstOrDefault(r => r.BookingId == b.BookingId)?.Rating ?? 0)
                        .DefaultIfEmpty(0)
                        .Average(),
                    TotalRevenue = 0
                })
                .OrderByDescending(p => p.CompletedBookings)
                .ThenByDescending(p => p.AverageRating)
                .Take(count)
                .ToList();
        }

        public async Task<IEnumerable<RevenueByMonthDto>> GetRevenueByMonthAsync(int months = 12)
        {
            var bookings = await _unitOfWork.Bookings.GetAllAsync();
            var startDate = DateTime.UtcNow.AddMonths(-months);

            return bookings
                .Where(b => b.CompletedDate >= startDate && b.Status == BookingStatus.Completed)
                .GroupBy(b => new { b.CompletedDate!.Value.Year, b.CompletedDate.Value.Month })
                .Select(g => new RevenueByMonthDto
                {
                    Year = g.Key.Year,
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Revenue = 0,
                    BookingCount = g.Count()
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => DateTime.ParseExact(r.Month, "MMM yyyy", null).Month)
                .ToList();
        }

        public async Task<IEnumerable<BookingStatusDistributionDto>> GetBookingStatusDistributionAsync()
        {
            var bookings = await _unitOfWork.Bookings.GetAllAsync();
            var total = bookings.Count();

            if (total == 0)
            {
                return Enumerable.Empty<BookingStatusDistributionDto>();
            }

            return bookings
                .GroupBy(b => b.Status)
                .Select(g => new BookingStatusDistributionDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = Math.Round((decimal)g.Count() / total * 100, 2)
                })
                .OrderByDescending(d => d.Count)
                .ToList();
        }

        public async Task<IEnumerable<UserGrowthDto>> GetUserGrowthAsync(int months = 12)
        {
            var clients = await _unitOfWork.Clients.GetAllAsync();
            var startDate = DateTime.UtcNow.AddMonths(-months);

            var monthlyData = clients
                .Where(c => c.DateRegistered >= startDate)
                .GroupBy(c => new { c.DateRegistered.Year, c.DateRegistered.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    NewUsers = g.Count(),
                    NewProviders = g.Count(c => c.IsProvider && c.ProviderStatus == ProviderStatus.Approved)
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToList();

            var result = new List<UserGrowthDto>();
            int cumulativeUsers = clients.Count(c => c.DateRegistered < startDate);

            foreach (var data in monthlyData)
            {
                cumulativeUsers += data.NewUsers;
                result.Add(new UserGrowthDto
                {
                    Year = data.Year,
                    Month = new DateTime(data.Year, data.Month, 1).ToString("MMM yyyy"),
                    NewUsers = data.NewUsers,
                    NewProviders = data.NewProviders,
                    TotalUsers = cumulativeUsers
                });
            }

            return result;
        }

        public async Task<IEnumerable<EmergencyStatsDto>> GetEmergencyStatsAsync(int days = 30)
        {
            var emergencies = await _unitOfWork.EmergencyRequests.GetAllAsync();
            var startDate = DateTime.UtcNow.AddDays(-days).Date;

            return emergencies
                .Where(e => e.RequestDateTime >= startDate)
                .GroupBy(e => e.RequestDateTime.Date)
                .Select(g => new EmergencyStatsDto
                {
                    Date = g.Key.ToString("MMM dd"),
                    TotalEmergencies = g.Count(),
                    AcceptedEmergencies = g.Count(e => e.Status == EmergencyStatus.Accepted),
                    PendingEmergencies = g.Count(e => e.Status == EmergencyStatus.Pending)
                })
                .OrderBy(e => DateTime.ParseExact(e.Date, "MMM dd", null))
                .ToList();
        }
    }
}
