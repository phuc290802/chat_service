namespace ChatApp.Domain.Entities;

public class UserConnection
{
    public Guid UserId { get; set; }
    public string ConnectionId { get; set; }
    public DateTime ConnectedAt { get; set; }
}