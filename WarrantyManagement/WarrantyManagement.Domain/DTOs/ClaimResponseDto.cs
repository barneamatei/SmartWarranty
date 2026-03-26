namespace WarrantyManagement.Domain.DTOs;

public class ClaimResponseDto
{
    public Guid ClaimId { get; set; }
    public Guid WarrantyId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string Description { get; set; } = string.Empty;
}
