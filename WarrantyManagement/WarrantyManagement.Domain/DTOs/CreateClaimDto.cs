using System.ComponentModel.DataAnnotations;

namespace WarrantyManagement.Domain.DTOs;

public class CreateClaimDto
{
    [Required]
    public Guid WarrantyId { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;
}
