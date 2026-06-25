using System.ComponentModel.DataAnnotations;

namespace DocumentAnalysis.Domain.DTOs;

public class CreateWarrantyDraftRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int DefaultDurationMonths { get; set; } = 24;
}
