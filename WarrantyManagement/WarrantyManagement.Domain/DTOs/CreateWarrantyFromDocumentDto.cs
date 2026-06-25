using System.ComponentModel.DataAnnotations;

namespace WarrantyManagement.Domain.DTOs;

public class CreateWarrantyFromDocumentDto
{
    [Required]
    public Guid DocumentId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; }

    [Range(1, int.MaxValue)]
    public int DurationMonths { get; set; }

    public string? ProductDescription { get; set; }

    public string? MerchantName { get; set; }

    public string? DocumentNumber { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Currency { get; set; }
}
