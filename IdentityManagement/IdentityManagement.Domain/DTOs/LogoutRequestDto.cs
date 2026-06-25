using System.ComponentModel.DataAnnotations;

namespace IdentityManagement.Domain.DTOs;

public class LogoutRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
