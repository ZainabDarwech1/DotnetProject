using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICategoryRepository : IRepository<ServiceCategory>
    {
        Task<IEnumerable<ServiceCategory>> GetActiveCategoriesAsync();
        Task<ServiceCategory?> GetCategoryWithServicesAsync(int categoryId);
    }
}