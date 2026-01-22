using DinkToPdf;
using DinkToPdf.Contracts;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using System.Text;

namespace LebAssist.Presentation.Services
{
    public class PdfService : IPdfService
    {
        private readonly IConverter _converter;

        public PdfService(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] GenerateProviderServiceReport(ProviderServiceReportDto reportData)
        {
            var htmlContent = GenerateHtmlContent(reportData);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 0, Bottom = 0, Left = 5, Right = 5 },
                DocumentTitle = $"Service Report - {reportData.ProviderName}"
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" },
            };

            var document = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            return _converter.Convert(document);
        }

        private string GenerateHtmlContent(ProviderServiceReportDto data)
        {
            var sb = new StringBuilder();

            sb.Append(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: #ffffff;
            color: #1a1d29;
            line-height: 1.5;
            padding: 0;
        }
        
        .container {
            max-width: 1000px;
            margin: 0 auto;
            padding: 40px 30px;
        }
        
        /* Header */
        .report-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            padding-bottom: 30px;
            border-bottom: 3px solid #4361ee;
            margin-bottom: 40px;
        }
        
        .company-info h1 {
            font-size: 28px;
            color: #4361ee;
            font-weight: 800;
            margin-bottom: 5px;
        }
        
        .company-info p {
            color: #6c757d;
            font-size: 14px;
        }
        
        .report-meta {
            text-align: right;
        }
        
        .report-title {
            font-size: 16px;
            color: #6c757d;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 8px;
        }
        
        .report-period {
            font-size: 14px;
            color: #1a1d29;
            font-weight: 600;
        }
        
        /* Provider Details Card */
        .provider-card {
            background: #f8f9fa;
            border-radius: 12px;
            padding: 30px;
            margin-bottom: 40px;
        }
        
        .provider-card h2 {
            font-size: 18px;
            color: #1a1d29;
            font-weight: 700;
            margin-bottom: 20px;
            padding-bottom: 12px;
            border-bottom: 2px solid #e9ecef;
        }
        
        .provider-details {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 20px;
        }
        
        .detail-item label {
            display: block;
            font-size: 12px;
            color: #6c757d;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 6px;
        }
        
        .detail-item span {
            display: block;
            font-size: 15px;
            color: #1a1d29;
            font-weight: 600;
        }
        
        /* Summary Cards */
        .summary-cards {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 20px;
            margin-bottom: 40px;
        }
        
        .summary-card {
            background: white;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            padding: 24px;
            text-align: center;
        }
        
        .summary-card.primary {
            border-color: #4361ee;
            background: #4361ee;
        }
        
        .summary-card.primary .card-label,
        .summary-card.primary .card-value {
            color: white;
        }
        
        .card-label {
            font-size: 13px;
            color: #6c757d;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 10px;
        }
        
        .card-value {
            font-size: 36px;
            color: #1a1d29;
            font-weight: 800;
        }
        
        /* Services Table */
        .services-section h2 {
            font-size: 20px;
            color: #1a1d29;
            font-weight: 700;
            margin-bottom: 20px;
        }
        
        table {
            width: 100%;
            border-collapse: separate;
            border-spacing: 0;
            background: white;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            overflow: hidden;
        }
        
        thead {
            background: #f8f9fa;
        }
        
        thead th {
            padding: 16px 20px;
            text-align: left;
            font-size: 12px;
            font-weight: 700;
            color: #6c757d;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            border-bottom: 2px solid #e9ecef;
        }
        
        tbody tr {
            border-bottom: 1px solid #f0f0f0;
        }
        
        tbody tr:last-child {
            border-bottom: none;
        }
        
        tbody td {
            padding: 18px 20px;
            font-size: 14px;
            color: #1a1d29;
        }
        
        .service-name {
            font-weight: 600;
        }
        
        .text-center {
            text-align: center;
        }
        
        .text-right {
            text-align: right;
        }
        
        .text-right {
            font-weight: 600;
        }
        
        .total-row {
            background: #f8f9fa;
            font-weight: 700;
        }
        
        .total-row td {
            padding: 20px;
            font-size: 15px;
            border-top: 2px solid #4361ee;
        }
        
        /* Footer */
        .report-footer {
            margin-top: 50px;
            padding-top: 30px;
            border-top: 2px solid #e9ecef;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        
        .footer-note {
            font-size: 12px;
            color: #6c757d;
            max-width: 600px;
        }
        
        .generation-date {
            text-align: right;
            font-size: 12px;
            color: #6c757d;
        }
        
        .generation-date strong {
            display: block;
            color: #1a1d29;
            margin-bottom: 4px;
        }
        
        /* Print Styles */
        @media print {
            body {
                padding: 0;
            }
            
            .container {
                padding: 20px;
            }
        }
    </style>
</head>
<body>
    <div class='container'>
        <!-- Header -->
        <div class='report-header'>
            <div class='company-info'>
                <h1>LebAssist</h1>
                <p>Professional Services Platform</p>
            </div>
            <div class='report-meta'>
                <div class='report-title'>Service Report</div>
                <div class='report-period'>
                    " + data.StartDate.ToString("MMM dd, yyyy") + @" - " + data.EndDate.ToString("MMM dd, yyyy") + @"
                </div>
            </div>
        </div>
        
        <!-- Provider Details -->
        <div class='provider-card'>
            <h2>Provider Information</h2>
            <div class='provider-details'>
                <div class='detail-item'>
                    <label>Full Name</label>
                    <span>" + data.ProviderName + @"</span>
                </div>
                <div class='detail-item'>
                    <label>Email Address</label>
                    <span>" + data.Email + @"</span>
                </div>
                <div class='detail-item'>
                    <label>Phone Number</label>
                    <span>" + data.PhoneNumber + @"</span>
                </div>
            </div>
        </div>
        
        <!-- Summary Cards -->
        <div class='summary-cards'>
            <div class='summary-card'>
                <div class='card-label'>Total Services</div>
                <div class='card-value'>" + data.TotalServicesProvided + @"</div>
            </div>
            <div class='summary-card primary'>
                <div class='card-label'>Total Revenue</div>
                <div class='card-value'>" + data.TotalRevenue + @" $</div>
            </div>
        </div>
        
        <!-- Services Table -->
        <div class='services-section'>
            <h2>Service Breakdown</h2>
            <table>
                <thead>
                    <tr>
                        <th>Service Name</th>
                        <th class='text-center'>Times Provided</th>
                        <th class='text-right'>Price per Service</th>
                        <th class='text-right'>Total Revenue</th>
                    </tr>
                </thead>
                <tbody>");

            // Add service rows
            foreach (var service in data.Services)
            {
                sb.Append($@"
                    <tr>
                        <td class='service-name'>{service.ServiceName}</td>
                        <td class='text-center'>{service.TimesProvided}</td>
                        <td class='text-right'>${service.PricePerService:N2}</td>
                        <td class='text-right'>${service.TotalRevenue:N2}</td>
                    </tr>");
            }

            sb.Append($@"
                    <tr class='total-row'>
                        <td>TOTAL</td>
                        <td class='text-center'>{data.TotalServicesProvided}</td>
                        <td></td>
                        <td class='text-right'>${data.TotalRevenue:N2}</td>
                    </tr>
                </tbody>
            </table>
        </div>
        
        <!-- Footer -->
        <div class='report-footer'>
            <div class='footer-note'>
                This report includes all completed services within the specified period. Revenue calculations are based on the service price at the time of completion.
            </div>
            <div class='generation-date'>
                <strong>Generated</strong>
                {data.ReportGeneratedDate.ToString("MMM dd, yyyy")}
            </div>
        </div>
    </div>
</body>
</html>");

            return sb.ToString();
        }
    }
}
