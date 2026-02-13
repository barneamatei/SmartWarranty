namespace UserManagement.Service.Exceptions;

public class FamilyShareAlreadyExistsException : Exception
{
    public FamilyShareAlreadyExistsException(Guid ownerUserId, Guid memberUserId)
        : base($"Family share already exists between owner {ownerUserId} and member {memberUserId}.")
    {
        OwnerUserId = ownerUserId;
        MemberUserId = memberUserId;
    }

    public Guid OwnerUserId { get; }

    public Guid MemberUserId { get; }
}
