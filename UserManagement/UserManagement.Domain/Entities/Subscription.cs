namespace UserManagement.Domain.Entities;

public class Subscription
{
    public Guid SubscriptionId { get; private set; }

    public Guid UserId { get; private set; }

    public PlanType PlanType { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public bool IsPremium => PlanType == PlanType.Premium;

    protected Subscription() { }

    public Subscription(Guid subscriptionId, Guid userId, PlanType planType, DateTime startDate, DateTime endDate)
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        PlanType = planType;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void UpgradeToPremium(DateTime newEndDate)
    {
        PlanType = PlanType.Premium;
        EndDate = newEndDate;
    }

    public void DowngradeToFree(DateTime newEndDate)
    {
        PlanType = PlanType.Free;
        EndDate = newEndDate;
    }
}
