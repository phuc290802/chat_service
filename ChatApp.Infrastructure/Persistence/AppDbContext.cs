namespace ChatApp.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using ChatApp.Domain.Entities;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserConnection> UserConnections => Set<UserConnection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============= USER =============
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // User <-> ConversationMember (1 - n)
        modelBuilder.Entity<ConversationMember>()
            .HasOne(cm => cm.User)
            .WithMany(u => u.ConversationMembers)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // User <-> Message (1 - n)
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.MessagesSent)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // User <-> RefreshToken (1 - n)
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        // ============= CONVERSATION =============
        // Conversation.CreatorUser (1 - n)
        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Cascade);

        // Conversation <-> ConversationMember (1 - n)
        modelBuilder.Entity<ConversationMember>()
            .HasOne(cm => cm.Conversation)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // ConversationMember composite key
        modelBuilder.Entity<ConversationMember>()
            .HasKey(cm => new { cm.ConversationId, cm.UserId });


        // ============= MESSAGE =============
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);


        // ============= ATTACHMENT =============
        modelBuilder.Entity<Attachment>()
            .HasOne(a => a.Message)
            .WithMany(m => m.Attachments)
            .HasForeignKey(a => a.MessageId)
            .OnDelete(DeleteBehavior.Cascade);


        // ============= USER CONNECTION =============
        modelBuilder.Entity<UserConnection>()
            .HasKey(uc => uc.UserId);
    }
}
