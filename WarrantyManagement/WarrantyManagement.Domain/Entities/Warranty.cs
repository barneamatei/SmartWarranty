namespace WarrantyManagement.Domain.Entities;

public class Warranty
{
    public Guid WarrantyId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public int DurationMonths { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public WarrantyStatus Status { get; private set; }
    public ICollection<Claim> Claims { get; private set; }

    protected Warranty()
    {
        Claims = new List<Claim>();
    }

    public Warranty(Guid warrantyId, Guid userId, Guid productId, DateTime purchaseDate, int durationMonths)
    {
        WarrantyId = warrantyId;
        Claims = new List<Claim>();
        UpdateDetails(userId, productId, purchaseDate, durationMonths);
        RecalculateStatus();
    }

    public void UpdateDetails(Guid userId, Guid productId, DateTime purchaseDate, int durationMonths)
    {
        UserId = userId;
        ProductId = productId;
        PurchaseDate = purchaseDate;
        DurationMonths = durationMonths;
        ExpiryDate = PurchaseDate.AddMonths(DurationMonths);
    }

    public WarrantyStatus RecalculateStatus(DateTime? referenceDate = null)
    {
        var comparisonDate = (referenceDate ?? DateTime.UtcNow).Date;
        if (Status == WarrantyStatus.Inactive)
            return Status;

        if (ExpiryDate.Date < comparisonDate)
        {
            Status = WarrantyStatus.Expired;
            return Status;
        }

        Status = Claims.Any(c => c.Status != ClaimStatus.Closed)
            ? WarrantyStatus.Claimed
            : WarrantyStatus.Active;

        return Status;
    }

    public void MarkInactive() => Status = WarrantyStatus.Inactive;

    public void MarkClaimed()
    {
        if (Status != WarrantyStatus.Inactive && Status != WarrantyStatus.Expired)
            Status = WarrantyStatus.Claimed;
    }

    public void MarkExpired() => Status = WarrantyStatus.Expired;
}
