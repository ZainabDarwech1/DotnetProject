using Domain.Entities;
using Domain.Interfaces;
using LebAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LebAssist.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<ServiceCategory>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ServiceCategory>> GetActiveCategoriesAsync()
        {
            return await _dbSet
                .Include(c => c.Services)
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<ServiceCategory?> GetCategoryWithServicesAsync(int categoryId)
        {
            return await _dbSet
                .Include(c => c.Services.Where(s => s.IsActive))
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }
    }
}