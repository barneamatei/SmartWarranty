namespace UserManagement.Service.DTOs;

public class FamilyShareResponseDto
{
    public Guid ShareId { get; set; }

    public Guid OwnerUserId { get; set; }

    public Guid MemberUserId { get; set; }

    public int Permissions { get; set; }
}
