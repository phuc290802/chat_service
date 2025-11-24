namespace ChatApp.Domain.Entities;

public class ConversationMember
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public string Role { get; set; } = "Member";

    public Conversation Conversation { get; set; } = default!;
    public User User { get; set; } = default!;
}

