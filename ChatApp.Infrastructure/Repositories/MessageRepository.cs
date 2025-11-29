using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Message> AddAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<Message> GetAsync(Guid messageId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Conversation)
            .FirstOrDefaultAsync(m => m.Id == messageId);
    }

    public async Task<List<Message>> GetConversationMessagesAsync(Guid conversationId, int page = 1, int pageSize = 50)
    {
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task UpdateAsync(Message message)
    {
        _context.Messages.Update(message);
        await _context.SaveChangesAsync();
    }

    public async Task MarkMessageAsSeenAsync(Guid messageId, Guid viewerId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null && message.SenderId != viewerId)
        {
            message.SeenAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Message>> GetUnreadMessagesAsync(Guid userId, Guid conversationId)
    {
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId
                     && m.SenderId != userId
                     && m.SeenAt == null)
            .ToListAsync();
    }

    public async Task<int> GetUnreadMessageCountAsync(Guid userId, Guid? conversationId = null)
    {
        var query = _context.Messages
            .Where(m => m.SenderId != userId && m.SeenAt == null);

        if (conversationId.HasValue)
        {
            query = query.Where(m => m.ConversationId == conversationId.Value);
        }

        return await query.CountAsync();
    }
}