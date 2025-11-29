namespace ChatApp.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string? Content { get; set; }
    public string MessageType { get; set; } = "Text";
    public DateTime CreatedAt { get; set; }
    public DateTime? SeenAt { get; set; }

    public Conversation Conversation { get; set; } = default!;
    public User Sender { get; set; } = default!;
    public ICollection<Attachment> Attachments { get; set; }
}

