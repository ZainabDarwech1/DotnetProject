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

        public async Task<EmergencyRequest?> GetWithDetailsAsync(int emergencyId)
        {
            return await _context.EmergencyRequests
                .Include(e => e.Client)
                .Include(e => e.Provider)
                .Include(e => e.Service)
                .FirstOrDefaultAsync(e => e.EmergencyRequestId == emergencyId);
        }

        public async Task<IEnumerable<EmergencyRequest>> GetByClientIdAsync(int clientId)
        {
            return await _context.EmergencyRequests
                .Include(e => e.Service)
                .Include(e => e.Provider)
                .Where(e => e.ClientId == clientId)
                .OrderByDescending(e => e.RequestDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyRequest>> GetPendingAsync()
        {
            return await _context.EmergencyRequests
                .Include(e => e.Client)
                .Include(e => e.Service)
                .Where(e => e.Status == EmergencyStatus.Pending && e.ProviderId == null)
                .OrderByDescending(e => e.RequestDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyRequest>> GetByProviderIdAsync(int providerId)
        {
            return await _context.EmergencyRequests
                .Include(e => e.Client)
                .Include(e => e.Service)
                .Where(e => e.ProviderId == providerId)
                .OrderByDescending(e => e.RequestDateTime)
                .ToListAsync();
        }
    }
}