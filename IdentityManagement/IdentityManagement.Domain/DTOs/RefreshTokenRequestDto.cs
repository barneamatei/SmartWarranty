using System.ComponentModel.DataAnnotations;

namespace IdentityManagement.Domain.DTOs;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
