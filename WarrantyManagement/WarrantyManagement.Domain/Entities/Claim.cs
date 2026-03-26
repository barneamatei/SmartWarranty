namespace WarrantyManagement.Domain.Entities;

public class Claim
{
    public Guid ClaimId { get; private set; }
    public Guid WarrantyId { get; private set; }
    public ClaimStatus Status { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string Description { get; private set; }
    public Warranty? Warranty { get; private set; }

    protected Claim()
    {
        Description = string.Empty;
    }

    public Claim(Guid claimId, Guid warrantyId, string description)
    {
        ClaimId = claimId;
        WarrantyId = warrantyId;
        Description = description;
        Status = ClaimStatus.Opened;
        OpenedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(ClaimStatus status)
    {
        Status = status;
        if (status != ClaimStatus.Closed)
            ClosedAt = null;
    }

    public void Close()
    {
        Status = ClaimStatus.Closed;
        ClosedAt = DateTime.UtcNow;
    }
}
