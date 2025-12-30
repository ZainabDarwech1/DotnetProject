using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LebAssist.Infrastructure.Repositories
{
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        public ClientRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Client?> GetByAspNetUserIdAsync(string userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.AspNetUserId == userId);
        }

        public async Task<IEnumerable<Client>> GetProvidersNearLocationAsync(decimal lat, decimal lon, int radiusKm)
        {
            // Simple distance calculation (for more accuracy, use Haversine formula in SQL)
            var providers = await _dbSet
                .Where(c => c.IsProvider && c.ProviderStatus == ProviderStatus.Approved)
                .ToListAsync();

            // Filter by distance (simplified - calculates approximate distance)
            return providers.Where(p =>
            {
                var distance = CalculateDistance(lat, lon, p.Latitude, p.Longitude);
                return distance <= radiusKm;
            });
        }

        public async Task<IEnumerable<Client>> GetProvidersByServiceAsync(int serviceId)
        {
            return await _dbSet
                .Include(c => c.ProviderServices)
                .Where(c => c.IsProvider &&
                           c.ProviderStatus == ProviderStatus.Approved &&
                           c.ProviderServices.Any(ps => ps.ServiceId == serviceId && ps.IsActive))
                .ToListAsync();
        }

        public async Task<IEnumerable<Client>> GetActiveProvidersAsync()
        {
            return await _dbSet
                .Where(c => c.IsProvider && c.ProviderStatus == ProviderStatus.Approved)
                .ToListAsync();
        }

        // Haversine formula for distance calculation
        private double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371; // Earth's radius in km

            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
        public async Task<bool> IsProviderAvailableAsync(int providerId)
        {
            var availability = await _context.ProviderAvailabilities
                .FirstOrDefaultAsync(a => a.ClientId == providerId);

            return availability?.IsAvailable ?? false;
        }
    }
}