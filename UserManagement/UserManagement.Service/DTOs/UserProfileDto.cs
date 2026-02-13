namespace UserManagement.Service.DTOs;

public class UserProfileDto
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Language { get; set; }

    public string? Preferences { get; set; }
}
