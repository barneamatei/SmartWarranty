namespace IdentityManagement.Domain.DTOs;

public class CreateUserProfileRequestDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Language { get; set; }
    public string? Preferences { get; set; }
}
