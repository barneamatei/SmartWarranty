using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.DTOs;

public class CreateFamilyShareDto
{
    [Required]
    public Guid OwnerUserId { get; set; }

    [Required]
    public Guid MemberUserId { get; set; }

    public int Permissions { get; set; }
}

