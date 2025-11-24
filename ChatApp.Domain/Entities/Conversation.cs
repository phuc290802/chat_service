namespace ChatApp.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool IsGroup { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public User CreatedByUser { get; set; } = default!;
    public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

