using System.ComponentModel.DataAnnotations;

namespace NotificationManagement.Domain.DTOs;

public class MarkNotificationFailedDto
{
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
