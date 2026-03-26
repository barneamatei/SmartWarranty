namespace UserManagement.Domain.DTOs;

public class SubscriptionResponseDto
{
    public Guid SubscriptionId { get; set; }

    public Guid UserId { get; set; }

    public string PlanType { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsPremium { get; set; }
}

