namespace UserManagement.Domain.DTOs;

public class UserResponseDto
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public UserProfileDto? UserProfile { get; set; }

    public SubscriptionResponseDto? Subscription { get; set; }
}

