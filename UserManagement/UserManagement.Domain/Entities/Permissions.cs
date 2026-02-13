namespace UserManagement.Domain.Entities;

[Flags]
public enum Permissions
{
    None = 0,
    View = 1,
    Edit = 2,
    Share = 4,
    FullAccess = View | Edit | Share
}
