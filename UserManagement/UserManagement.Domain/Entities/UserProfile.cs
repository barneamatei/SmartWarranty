namespace UserManagement.Domain.Entities;

public class UserProfile
{
    public Guid UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public string? Language { get; private set; }

    public string? Preferences { get; private set; }

    protected UserProfile() { }

    public UserProfile(Guid userId, string name, string? phone = null, string? language = null, string? preferences = null)
    {
        UserId = userId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Phone = phone;
        Language = language;
        Preferences = preferences;
    }

    public void Update(string name, string? phone, string? language, string? preferences)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Phone = phone;
        Language = language;
        Preferences = preferences;
    }
}
