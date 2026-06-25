namespace WarrantyManagement.Domain.DTOs;

public class DocumentWarrantyDraftDto
{
    public Guid DocumentId { get; set; }

    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }

    public DateTime PurchaseDate { get; set; }

    public int DurationMonths { get; set; }

    public string? ProductDescription { get; set; }

    public string? MerchantName { get; set; }

    public string? DocumentNumber { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Currency { get; set; }
}
