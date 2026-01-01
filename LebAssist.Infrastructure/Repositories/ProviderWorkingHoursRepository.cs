using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LebAssist.Infrastructure.Repositories
{
    public class ProviderWorkingHoursRepository : GenericRepository<ProviderWorkingHours>, IProviderWorkingHoursRepository
    {
        public ProviderWorkingHoursRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ProviderWorkingHours>> GetByProviderIdAsync(int providerId)
        {
            return await _dbSet
                .Include(wh => wh.Service)
                .Where(wh => wh.ClientId == providerId)
                .OrderBy(wh => wh.DayOfWeek)
                .ThenBy(wh => wh.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProviderWorkingHours>> GetByProviderAndServiceAsync(int providerId, int serviceId)
        {
            return await _dbSet
                .Where(wh => wh.ClientId == providerId && wh.ServiceId == serviceId && wh.IsActive)
                .OrderBy(wh => wh.DayOfWeek)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProviderWorkingHours>> GetByProviderAndDayAsync(int providerId, DayOfWeekEnum day)
        {
            return await _dbSet
                .Include(wh => wh.Service)
                .Where(wh => wh.ClientId == providerId && wh.DayOfWeek == day && wh.IsActive)
                .ToListAsync();
        }
    }
}