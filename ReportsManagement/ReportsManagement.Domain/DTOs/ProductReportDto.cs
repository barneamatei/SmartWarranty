namespace ReportsManagement.Domain.DTOs;

public sealed class ProductReportDto
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
