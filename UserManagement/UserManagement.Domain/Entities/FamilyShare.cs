namespace UserManagement.Domain.Entities;

public class FamilyShare
{
    public Guid ShareId { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public Guid MemberUserId { get; private set; }

    public Permissions Permissions { get; private set; }

    protected FamilyShare() { }

    public FamilyShare(Guid shareId, Guid ownerUserId, Guid memberUserId, Permissions permissions)
    {
        if (ownerUserId == memberUserId)
            throw new ArgumentException("A user cannot share with themselves.", nameof(memberUserId));

        ShareId = shareId;
        OwnerUserId = ownerUserId;
        MemberUserId = memberUserId;
        Permissions = permissions;
    }

    public void UpdatePermissions(Permissions permissions)
    {
        Permissions = permissions;
    }
}
