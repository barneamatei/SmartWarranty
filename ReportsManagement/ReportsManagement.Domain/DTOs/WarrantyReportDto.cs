namespace ReportsManagement.Domain.DTOs;

public sealed class WarrantyReportDto
{
    public Guid WarrantyId { get; init; }
    public Guid UserId { get; init; }
    public Guid ProductId { get; init; }
    public DateTime PurchaseDate { get; init; }
    public int DurationMonths { get; init; }
    public DateTime ExpiryDate { get; init; }
    public string Status { get; init; } = string.Empty;
}
