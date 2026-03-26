using System.ComponentModel.DataAnnotations;

namespace WarrantyManagement.Domain.DTOs;

public class UpdateClaimStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
