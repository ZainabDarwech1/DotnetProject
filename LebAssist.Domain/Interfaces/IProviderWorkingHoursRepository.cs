using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public interface IProviderWorkingHoursRepository : IRepository<ProviderWorkingHours>
    {
        Task<IEnumerable<ProviderWorkingHours>> GetByProviderIdAsync(int providerId);
        Task<IEnumerable<ProviderWorkingHours>> GetByProviderAndServiceAsync(int providerId, int serviceId);
        Task<IEnumerable<ProviderWorkingHours>> GetByProviderAndDayAsync(int providerId, DayOfWeekEnum day);
    }
}