using Domain.Entities;
using Domain.Interfaces;
using LebAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LebAssist.Infrastructure.Repositories
{
    public class ProviderServiceRepository : GenericRepository<ProviderServiceEntity>, IProviderServiceRepository
    {
        public ProviderServiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ProviderServiceEntity>> GetByProviderIdAsync(int providerId)
        {
            return await _dbSet
                .Include(ps => ps.Service)
                    .ThenInclude(s => s.Category)
                .Where(ps => ps.ClientId == providerId && ps.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProviderServiceEntity>> GetAllByProviderIdAsync(int providerId)
        {
            return await _dbSet
                .Include(ps => ps.Service)
                    .ThenInclude(s => s.Category)
                .Where(ps => ps.ClientId == providerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProviderServiceEntity>> GetByServiceIdAsync(int serviceId)
        {
            return await _dbSet
                .Include(ps => ps.Provider)
                .Where(ps => ps.ServiceId == serviceId && ps.IsActive)
                .ToListAsync();
        }

        public async Task<ProviderServiceEntity?> GetByProviderAndServiceAsync(int providerId, int serviceId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ps => ps.ClientId == providerId && ps.ServiceId == serviceId);
        }
    }
}