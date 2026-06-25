namespace ReportsManagement.Domain.DTOs;

public sealed class UserReportDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Language { get; init; }
    public string? SubscriptionPlan { get; init; }
    public DateTime? SubscriptionEndDate { get; init; }
    public bool IsPremium { get; init; }
}
