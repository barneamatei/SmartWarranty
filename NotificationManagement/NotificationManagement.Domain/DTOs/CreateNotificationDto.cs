using System.ComponentModel.DataAnnotations;

namespace NotificationManagement.Domain.DTOs;

public class CreateNotificationDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Channel { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Metadata { get; set; }
}
