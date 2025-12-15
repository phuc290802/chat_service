using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs
{
    public class ConversationDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public bool IsGroup { get; set; }
        public IEnumerable<ConversationMemberDto> Members { get; set; } = new List<ConversationMemberDto>();
    }

    public class ConversationMemberDto
    {
        public Guid UserId { get; set; }
        public string Role { get; set; }
    }

    public record ConverstationRespone(Guid id, string name, bool isGroup, bool isDirectMessage, string createdBy, DateTime createdAt, string? lastMessage, string avatarUrl, List<MemberConverstationRespone> members, int unreadCount = 0);

    public record MemberConverstationRespone(Guid conversationId, DateTime joinedAt, UserDto user, string role = "member");
}
