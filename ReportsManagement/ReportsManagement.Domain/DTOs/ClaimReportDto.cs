namespace ReportsManagement.Domain.DTOs;

public sealed class ClaimReportDto
{
    public Guid ClaimId { get; init; }
    public Guid WarrantyId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime OpenedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public string Description { get; init; } = string.Empty;
}
