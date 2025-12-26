using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LebAssist.Infrastructure.Repositories
{
    public class ReportRepository : GenericRepository<Report>, IReportRepository
    {
        public ReportRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Report>> GetPendingReportsAsync()
        {
            return await _dbSet
                .Include(r => r.Reporter)
                .Include(r => r.ReportedProvider)
                .Where(r => r.Status == ReportStatus.Pending)
                .OrderBy(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetReportsByProviderAsync(int providerId)
        {
            return await _dbSet
                .Include(r => r.Reporter)
                .Include(r => r.Booking)
                .Where(r => r.ReportedProviderId == providerId)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetReportsByStatusAsync(ReportStatus status)
        {
            return await _dbSet
                .Include(r => r.Reporter)
                .Include(r => r.ReportedProvider)
                .Where(r => r.Status == status)
                .ToListAsync();
        }

        public async Task<Report?> GetReportWithDetailsAsync(int reportId)
        {
            return await _dbSet
                .Include(r => r.Reporter)
                .Include(r => r.ReportedProvider)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);
        }
    }
}