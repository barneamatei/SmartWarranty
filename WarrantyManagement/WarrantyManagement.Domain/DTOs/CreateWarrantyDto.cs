using System.ComponentModel.DataAnnotations;

namespace WarrantyManagement.Domain.DTOs;

public class CreateWarrantyDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; }

    [Range(1, int.MaxValue)]
    public int DurationMonths { get; set; }
}
