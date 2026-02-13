using System.ComponentModel.DataAnnotations;

namespace UserManagement.Service.DTOs;

public class UpdateUserDto
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(10)]
    public string? Language { get; set; }

    [StringLength(500)]
    public string? Preferences { get; set; }
}
