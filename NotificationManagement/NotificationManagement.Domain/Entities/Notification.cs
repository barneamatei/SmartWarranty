namespace NotificationManagement.Domain.Entities;

public class Notification
{
    public Guid NotificationId { get; private set; }

    public Guid UserId { get; private set; }

    public string Title { get; private set; }

    public string Message { get; private set; }

    public NotificationType Type { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public NotificationStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? SentAt { get; private set; }

    public DateTime? ReadAt { get; private set; }

    public string? Metadata { get; private set; }

    public string? ErrorMessage { get; private set; }

    private Notification()
    {
        Title = string.Empty;
        Message = string.Empty;
    }

    public Notification(
        Guid notificationId,
        Guid userId,
        string title,
        string message,
        NotificationType type,
        NotificationChannel channel,
        string? metadata = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is invalid.", nameof(userId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required.", nameof(message));

        NotificationId = notificationId;
        UserId = userId;
        Title = title.Trim();
        Message = message.Trim();
        Type = type;
        Channel = channel;
        Status = NotificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        Metadata = string.IsNullOrWhiteSpace(metadata) ? null : metadata.Trim();
    }

    public void MarkSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkRead()
    {
        if (Status == NotificationStatus.Failed)
            throw new InvalidOperationException("A failed notification cannot be marked as read.");

        Status = NotificationStatus.Read;
        if (!SentAt.HasValue)
            SentAt = DateTime.UtcNow;
        ReadAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkFailed(string? errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Notification delivery failed." : errorMessage.Trim();
    }
}
