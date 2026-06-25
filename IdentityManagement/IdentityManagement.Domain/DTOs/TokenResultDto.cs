namespace IdentityManagement.Domain.DTOs;

public class TokenResultDto
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}
