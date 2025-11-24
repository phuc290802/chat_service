namespace ChatApp.Domain.Entities;

public class Attachment
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public string FileUrl { get; set; } = default!;
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }

    // Navigation
    public Message Message { get; set; } = null!;
}
