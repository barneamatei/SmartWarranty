namespace WarrantyManagement.Domain.DTOs;

public class WarrantyResponseDto
{
    public Guid WarrantyId { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int DurationMonths { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
