namespace UserManagement.Domain.Entities;

public class User
{
    public Guid UserId { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; }

    public UserProfile? UserProfile { get; private set; }

    public Subscription? Subscription { get; private set; }

    protected User() { }

    public User(Guid userId, string email, UserStatus status = UserStatus.Active)
    {
        UserId = userId;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Status = status;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
    }

    public void UpdateEmail(string email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public void SetProfile(UserProfile profile)
    {
        UserProfile = profile ?? throw new ArgumentNullException(nameof(profile));
    }

    public void SetSubscription(Subscription subscription)
    {
        Subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
    }
}
