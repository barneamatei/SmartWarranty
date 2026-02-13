using System.ComponentModel.DataAnnotations;

namespace UserManagement.Service.DTOs;

public class UpdateSubscriptionDto
{
    public string PlanType { get; set; } = "Free";

    [Required]
    public DateTime EndDate { get; set; }
}
