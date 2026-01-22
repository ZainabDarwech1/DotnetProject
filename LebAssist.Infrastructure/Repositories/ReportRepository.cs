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
                    .ThenInclude(b => b.Service)
                .Where(r => r.ReportedProviderId == providerId)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetReportsByStatusAsync(ReportStatus status)
        {
            return await _dbSet
                .Include(r => r.Reporter)
                .Include(r => r.ReportedProvider)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .Where(r => r.Status == status)
                .ToListAsync();
        }

        public async Task<Report?> GetReportWithDetailsAsync(int reportId)
        {
            return await _dbSet
                .Include(r => r.Reporter)
                .Include(r => r.ReportedProvider)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);
        }

        public async Task<IEnumerable<Report>> GetByReporterIdAsync(int reporterId)
        {
            return await _dbSet
                .Include(r => r.ReportedProvider)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .Where(r => r.ReporterId == reporterId)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetByProviderIdAsync(int providerId)
        {
            return await GetReportsByProviderAsync(providerId);
        }

        public async Task<IEnumerable<Report>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(r => r.Reporter)
                .Include(r => r.ReportedProvider)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<Report?> GetByIdWithDetailsAsync(int reportId)
        {
            return await GetReportWithDetailsAsync(reportId);
        }
    }
}