using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateProviderServiceReport(ProviderServiceReportDto reportData);
    }
}
