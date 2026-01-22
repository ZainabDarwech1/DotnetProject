using Domain.Entities;
using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IReportService
    {
        // Client operations
        Task<int> CreateReportAsync(int clientId, CreateReportDto dto);
        Task<IEnumerable<ReportDto>> GetClientReportsAsync(int clientId);
        Task<ReportDto?> GetReportByIdAsync(int reportId);

        // Admin operations
        Task<IEnumerable<ReportDto>> GetAllReportsAsync(string? status = null);
        Task<bool> UpdateReportStatusAsync(UpdateReportStatusDto dto, string adminUsername);
    }
}
