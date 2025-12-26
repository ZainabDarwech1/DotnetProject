using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LebAssist.Infrastructure.Repositories
{
    public class EmergencyRequestRepository : GenericRepository<EmergencyRequest>, IEmergencyRequestRepository
    {
        public EmergencyRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<EmergencyRequest>> GetActiveEmergencyRequestsAsync()
        {
            return await _dbSet
                .Include(e => e.Client)
                .Include(e => e.Service)
                .Where(e => e.Status == EmergencyStatus.Pending)
                .OrderByDescending(e => e.RequestDateTime)
                .ToListAsync();
        }

        public async Task<EmergencyRequest?> GetEmergencyByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Client)
                .Include(e => e.Service)
                .Include(e => e.Provider)
                .FirstOrDefaultAsync(e => e.EmergencyRequestId == id);
        }

        public async Task<IEnumerable<EmergencyRequest>> GetClientEmergenciesAsync(int clientId)
        {
            return await _dbSet
                .Include(e => e.Service)
                .Include(e => e.Provider)
                .Where(e => e.ClientId == clientId)
                .OrderByDescending(e => e.RequestDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyRequest>> GetProviderEmergenciesAsync(int providerId)
        {
            return await _dbSet
                .Include(e => e.Client)
                .Include(e => e.Service)
                .Where(e => e.ProviderId == providerId)
                .OrderByDescending(e => e.RequestDateTime)
                .ToListAsync();
        }
    }
}