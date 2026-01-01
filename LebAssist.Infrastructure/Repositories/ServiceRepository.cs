using Domain.Entities;
using Domain.Interfaces;
using LebAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LebAssist.Infrastructure.Repositories
{
    public class ServiceRepository : GenericRepository<Service>, IServiceRepository
    {
        public ServiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Service?> GetByIdWithCategoryAsync(int serviceId)
        {
            return await _context.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);
        }

        public async Task<IEnumerable<Service>> GetByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Where(s => s.CategoryId == categoryId && s.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> GetActiveServicesAsync()
        {
            return await _dbSet
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .ToListAsync();
        }

        public async Task<Service?> GetServiceWithProvidersAsync(int serviceId)
        {
            return await _dbSet
                .Include(s => s.ProviderServices)
                    .ThenInclude(ps => ps.Provider)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);
        }
    }
}