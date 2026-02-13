using System.ComponentModel.DataAnnotations;

namespace UserManagement.Service.DTOs;

public class CreateSubscriptionDto
{
    [Required]
    public Guid UserId { get; set; }

    public string PlanType { get; set; } = "Free";

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}
