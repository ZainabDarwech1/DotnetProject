using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public interface IReportRepository : IRepository<Report>
    {
        Task<IEnumerable<Report>> GetPendingReportsAsync();
        Task<IEnumerable<Report>> GetReportsByProviderAsync(int providerId);
        Task<IEnumerable<Report>> GetReportsByStatusAsync(ReportStatus status);
        Task<Report?> GetReportWithDetailsAsync(int reportId);
        Task<IEnumerable<Report>> GetByReporterIdAsync(int reporterId);
        Task<IEnumerable<Report>> GetByProviderIdAsync(int providerId);
        Task<IEnumerable<Report>> GetAllWithDetailsAsync();
        Task<Report?> GetByIdWithDetailsAsync(int reportId);
    }
}