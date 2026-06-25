namespace WarrantyManagement.Domain.DTOs;

public class WarrantyCreationFromDocumentResponseDto
{
    public Guid DocumentId { get; set; }

    public string? MerchantName { get; set; }

    public string? DocumentNumber { get; set; }

    public string? ProductDescription { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Currency { get; set; }

    public WarrantyResponseDto Warranty { get; set; } = new();
}
