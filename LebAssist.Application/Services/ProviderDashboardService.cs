using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LebAssist.Application.Services
{
    public class ProviderDashboardService : IProviderDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProviderDashboardService> _logger;

        public ProviderDashboardService(
            IUnitOfWork unitOfWork,
            ILogger<ProviderDashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ProviderDashboardStatsDto> GetProviderDashboardStatsAsync(int providerId)
        {
            try
            {
                var allBookings = await _unitOfWork.Bookings.GetAllAsync();
                var providerBookings = allBookings.Where(b => b.ProviderId == providerId).ToList();

                var allReviews = await _unitOfWork.Reviews.GetAllAsync();
                var providerReviews = allReviews
                    .Where(r => providerBookings.Any(b => b.BookingId == r.BookingId))
                    .ToList();

                var allServices = await _unitOfWork.ProviderServices.GetAllAsync();
                var providerServices = allServices.Where(ps => ps.ClientId == providerId).ToList();

                var now = DateTime.UtcNow;
                var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                var lastMonthStart = thisMonthStart.AddMonths(-1);

                var bookingsThisMonth = providerBookings.Count(b => b.ScheduledDateTime >= thisMonthStart);
                var bookingsLastMonth = providerBookings.Count(b => b.ScheduledDateTime >= lastMonthStart && b.ScheduledDateTime < thisMonthStart);

                var completedBookings = providerBookings.Where(b => b.Status == BookingStatus.Completed).ToList();

                // Calculate revenue (Price per hour * hours worked)
                var totalRevenue = completedBookings.Sum(b => CalculateBookingRevenue(b, providerServices));
                var revenueThisMonth = completedBookings
                    .Where(b => b.ScheduledDateTime >= thisMonthStart)
                    .Sum(b => CalculateBookingRevenue(b, providerServices));
                var revenueLastMonth = completedBookings
                    .Where(b => b.ScheduledDateTime >= lastMonthStart && b.ScheduledDateTime < thisMonthStart)
                    .Sum(b => CalculateBookingRevenue(b, providerServices));

                var newReviewsThisMonth = providerReviews.Count(r => r.ReviewDate >= thisMonthStart);

                return new ProviderDashboardStatsDto
                {
                    TotalBookings = providerBookings.Count,
                    PendingBookings = providerBookings.Count(b => b.Status == BookingStatus.Pending),
                    AcceptedBookings = providerBookings.Count(b => b.Status == BookingStatus.Accepted || b.Status == BookingStatus.InProgress),
                    CompletedBookings = completedBookings.Count,
                    CancelledBookings = providerBookings.Count(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Rejected),
                    TotalServices = providerServices.Count,
                    ActiveServices = providerServices.Count(ps => ps.IsActive),
                    TotalRevenue = totalRevenue,
                    RevenueThisMonth = revenueThisMonth,
                    RevenueLastMonth = revenueLastMonth,
                    AverageRating = providerReviews.Any() ? (decimal)providerReviews.Average(r => r.Rating) : 0,
                    TotalReviews = providerReviews.Count,
                    NewReviewsThisMonth = newReviewsThisMonth,
                    BookingsThisMonth = bookingsThisMonth,
                    BookingsLastMonth = bookingsLastMonth,
                    AverageRevenuePerBooking = completedBookings.Count > 0 ? totalRevenue / completedBookings.Count : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider dashboard stats for provider {ProviderId}", providerId);
                return new ProviderDashboardStatsDto();
            }
        }

        public async Task<IEnumerable<ProviderBookingTrendDto>> GetProviderBookingTrendsAsync(int providerId, int months = 6)
        {
            try
            {
                var allBookings = await _unitOfWork.Bookings.GetAllAsync();
                var providerBookings = allBookings.Where(b => b.ProviderId == providerId).ToList();

                var allServices = await _unitOfWork.ProviderServices.GetAllAsync();
                var providerServices = allServices.Where(ps => ps.ClientId == providerId).ToList();

                var startDate = DateTime.UtcNow.AddMonths(-months);

                var trends = providerBookings
                    .Where(b => b.ScheduledDateTime >= startDate)
                    .GroupBy(b => new { b.ScheduledDateTime.Year, b.ScheduledDateTime.Month })
                    .Select(g => new ProviderBookingTrendDto
                    {
                        Year = g.Key.Year,
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                        TotalBookings = g.Count(),
                        CompletedBookings = g.Count(b => b.Status == BookingStatus.Completed),
                        Revenue = g.Where(b => b.Status == BookingStatus.Completed)
                                   .Sum(b => CalculateBookingRevenue(b, providerServices))
                    })
                    .OrderBy(t => t.Year)
                    .ThenBy(t => DateTime.ParseExact(t.Month, "MMM", System.Globalization.CultureInfo.InvariantCulture).Month)
                    .ToList();

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider booking trends for provider {ProviderId}", providerId);
                return Enumerable.Empty<ProviderBookingTrendDto>();
            }
        }

        public async Task<IEnumerable<ProviderRevenueByMonthDto>> GetProviderRevenueByMonthAsync(int providerId, int months = 12)
        {
            try
            {
                var allBookings = await _unitOfWork.Bookings.GetAllAsync();
                var providerBookings = allBookings
                    .Where(b => b.ProviderId == providerId && b.Status == BookingStatus.Completed)
                    .ToList();

                var allServices = await _unitOfWork.ProviderServices.GetAllAsync();
                var providerServices = allServices.Where(ps => ps.ClientId == providerId).ToList();

                var startDate = DateTime.UtcNow.AddMonths(-months);

                var revenue = providerBookings
                    .Where(b => b.ScheduledDateTime >= startDate)
                    .GroupBy(b => new { b.ScheduledDateTime.Year, b.ScheduledDateTime.Month })
                    .Select(g =>
                    {
                        var bookings = g.ToList();
                        var totalRevenue = bookings.Sum(b => CalculateBookingRevenue(b, providerServices));
                        return new ProviderRevenueByMonthDto
                        {
                            Year = g.Key.Year,
                            Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                            Revenue = totalRevenue,
                            BookingCount = bookings.Count,
                            AveragePerBooking = bookings.Count > 0 ? totalRevenue / bookings.Count : 0
                        };
                    })
                    .OrderBy(r => r.Year)
                    .ThenBy(r => DateTime.ParseExact(r.Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month)
                    .ToList();

                return revenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider revenue by month for provider {ProviderId}", providerId);
                return Enumerable.Empty<ProviderRevenueByMonthDto>();
            }
        }

        public async Task<IEnumerable<ProviderServiceRevenueDto>> GetProviderServiceRevenueAsync(int providerId)
        {
            try
            {
                var allBookings = await _unitOfWork.Bookings.GetAllAsync();
                var providerBookings = allBookings.Where(b => b.ProviderId == providerId).ToList();

                var allServices = await _unitOfWork.ProviderServices.GetByProviderIdAsync(providerId);
                var providerServices = allServices.Where(ps => ps.ClientId == providerId).ToList();

                var allReviews = await _unitOfWork.Reviews.GetAllAsync();

                var serviceRevenue = providerBookings
                    .GroupBy(b => b.ServiceId)
                    .Select(g =>
                    {
                        var service = providerServices.FirstOrDefault(ps => ps.ServiceId == g.Key);
                        var completedBookings = g.Where(b => b.Status == BookingStatus.Completed).ToList();
                        var serviceReviews = allReviews
                            .Where(r => completedBookings.Any(b => b.BookingId == r.BookingId))
                            .ToList();

                        return new ProviderServiceRevenueDto
                        {
                            ServiceId = g.Key,
                            ServiceName = service?.Service?.ServiceName ?? "Unknown",
                            CategoryName = service?.Service?.Category?.CategoryName ?? "Unknown",
                            PricePerHour = service?.PricePerHour ?? 0,
                            BookingCount = g.Count(),
                            CompletedBookings = completedBookings.Count,
                            TotalRevenue = completedBookings.Sum(b => CalculateBookingRevenue(b, providerServices)),
                            AverageRating = serviceReviews.Any() ? (decimal)serviceReviews.Average(r => r.Rating) : 0
                        };
                    })
                    .OrderByDescending(s => s.TotalRevenue)
                    .ToList();

                return serviceRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider service revenue for provider {ProviderId}", providerId);
                return Enumerable.Empty<ProviderServiceRevenueDto>();
            }
        }

        public async Task<IEnumerable<ProviderBookingStatusDto>> GetProviderBookingStatusDistributionAsync(int providerId)
        {
            try
            {
                var allBookings = await _unitOfWork.Bookings.GetAllAsync();
                var providerBookings = allBookings.Where(b => b.ProviderId == providerId).ToList();

                if (!providerBookings.Any())
                    return Enumerable.Empty<ProviderBookingStatusDto>();

                var total = providerBookings.Count;

                var statusDistribution = providerBookings
                    .GroupBy(b => b.Status)
                    .Select(g => new ProviderBookingStatusDto
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        Percentage = (decimal)g.Count() / total * 100
                    })
                    .ToList();

                return statusDistribution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider booking status distribution for provider {ProviderId}", providerId);
                return Enumerable.Empty<ProviderBookingStatusDto>();
            }
        }

        public async Task<IEnumerable<ProviderRecentBookingDto>> GetProviderRecentBookingsAsync(int providerId, int count = 5)
        {
            try
            {
                var allBookings = await _unitOfWork.Bookings.GetProviderBookingsAsync(providerId);
                var providerBookings = allBookings
                    .Where(b => b.ProviderId == providerId)
                    .OrderByDescending(b => b.ScheduledDateTime)
                    .Take(count)
                    .ToList();

                var allServices = await _unitOfWork.ProviderServices.GetAllAsync();
                var providerServices = allServices.Where(ps => ps.ClientId == providerId).ToList();

                var recentBookings = providerBookings.Select(b => new ProviderRecentBookingDto
                {
                    BookingId = b.BookingId,
                    ClientName = b.Client != null ? $"{b.Client.FirstName} {b.Client.LastName}" : "Unknown",
                    ServiceName = b.Service?.ServiceName ?? "Unknown",
                    ScheduledDateTime = b.ScheduledDateTime,
                    Status = b.Status.ToString(),
                    EstimatedRevenue = b.Status == BookingStatus.Completed ? CalculateBookingRevenue(b, providerServices) : 0
                }).ToList();

                return recentBookings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider recent bookings for provider {ProviderId}", providerId);
                return Enumerable.Empty<ProviderRecentBookingDto>();
            }
        }

        private decimal CalculateBookingRevenue(Domain.Entities.Booking booking, List<Domain.Entities.ProviderServiceEntity> providerServices)
        {
            var service = providerServices.FirstOrDefault(ps => ps.ServiceId == booking.ServiceId);
            if (service == null || !service.PricePerHour.HasValue)
                return 0;

            // Assuming 1 hour minimum or calculate based on booking duration if available
            // You can enhance this based on your business logic
            var hours = 1; // Default to 1 hour

            // If you have duration field in booking, use it:
            // var hours = (booking.EndDateTime - booking.StartDateTime).TotalHours;

            return service.PricePerHour.Value * hours;
        }

        public async Task<ProviderCalendarDto> GetProviderCalendarAsync(int providerId, int year, int month)
        {
            try
            {
                var allBookings = await _unitOfWork.Bookings.GetProviderBookingsAsync(providerId);
                var monthStart = new DateTime(year, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                // Get bookings for the month
                var monthBookings = allBookings
                    .Where(b => b.ScheduledDateTime >= monthStart &&
                                b.ScheduledDateTime <= monthEnd.AddDays(1).AddSeconds(-1))
                    .OrderBy(b => b.ScheduledDateTime)
                    .ToList();

                // Build calendar days
                var days = new List<CalendarDayDto>();
                var firstDayOfMonth = monthStart;
                var lastDayOfMonth = monthEnd;

                // Add days from previous month to fill the first week
                var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
                var previousMonthStart = monthStart.AddDays(-firstDayOfWeek);

                for (int i = 0; i < firstDayOfWeek; i++)
                {
                    var date = previousMonthStart.AddDays(i);
                    days.Add(new CalendarDayDto
                    {
                        Day = date.Day,
                        Date = date,
                        IsToday = date.Date == DateTime.Today,
                        IsCurrentMonth = false,
                        AppointmentCount = 0,
                        DayAppointments = new List<AppointmentDto>()
                    });
                }

                // Add current month days
                for (int day = 1; day <= lastDayOfMonth.Day; day++)
                {
                    var date = new DateTime(year, month, day);
                    var dayBookings = monthBookings
                        .Where(b => b.ScheduledDateTime.Date == date.Date)
                        .Select(b => MapToAppointmentDto(b))
                        .ToList();

                    days.Add(new CalendarDayDto
                    {
                        Day = day,
                        Date = date,
                        IsToday = date.Date == DateTime.Today,
                        IsCurrentMonth = true,
                        AppointmentCount = dayBookings.Count,
                        DayAppointments = dayBookings
                    });
                }

                // Add days from next month to complete the last week
                var remainingDays = 42 - days.Count; // 6 weeks * 7 days
                var nextMonthStart = monthEnd.AddDays(1);

                for (int i = 0; i < remainingDays; i++)
                {
                    var date = nextMonthStart.AddDays(i);
                    days.Add(new CalendarDayDto
                    {
                        Day = date.Day,
                        Date = date,
                        IsToday = date.Date == DateTime.Today,
                        IsCurrentMonth = false,
                        AppointmentCount = 0,
                        DayAppointments = new List<AppointmentDto>()
                    });
                }

                var appointments = monthBookings.Select(b => MapToAppointmentDto(b)).ToList();

                return new ProviderCalendarDto
                {
                    Year = year,
                    Month = month,
                    MonthName = monthStart.ToString("MMMM yyyy"),
                    Days = days,
                    Appointments = appointments
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider calendar for provider {ProviderId}", providerId);
                return new ProviderCalendarDto
                {
                    Year = year,
                    Month = month,
                    MonthName = new DateTime(year, month, 1).ToString("MMMM yyyy"),
                    Days = new List<CalendarDayDto>(),
                    Appointments = new List<AppointmentDto>()
                };
            }
        }

        public async Task<List<AppointmentDto>> GetProviderAppointmentsForDayAsync(int providerId, DateTime date)
        {
            try
            {
                var allBookings = await _unitOfWork.Bookings.GetProviderBookingsAsync(providerId);
                var dayBookings = allBookings
                    .Where(b => b.ScheduledDateTime.Date == date.Date)
                    .OrderBy(b => b.ScheduledDateTime)
                    .ToList();

                return dayBookings.Select(b => MapToAppointmentDto(b)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider appointments for day {Date}", date);
                return new List<AppointmentDto>();
            }
        }

        private AppointmentDto MapToAppointmentDto(Domain.Entities.Booking booking)
        {
            return new AppointmentDto
            {
                BookingId = booking.BookingId,
                ClientName = booking.Client != null ? $"{booking.Client.FirstName} {booking.Client.LastName}" : "Unknown",
                ClientPhoto = booking.Client?.ProfilePhotoPath,
                ClientPhone = booking.Client?.PhoneNumber,
                ServiceName = booking.Service?.ServiceName ?? "Unknown",
                ScheduledDateTime = booking.ScheduledDateTime,
                Status = booking.Status.ToString(),
                Notes = booking.Notes,
                Latitude = booking.Latitude,
                Longitude = booking.Longitude
            };
        }
    }
}
