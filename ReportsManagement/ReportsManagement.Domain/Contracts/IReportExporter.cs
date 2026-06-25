using ReportsManagement.Domain.DTOs;

namespace ReportsManagement.Domain.Contracts;

public interface IReportExporter
{
    string Format { get; }
    string ContentType { get; }
    byte[] Export(ReportDefinition report);
}
