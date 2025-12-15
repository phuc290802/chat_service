using ChatApp.Application.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Services
{
    public class UserConnectionService : IUserConnectionService
    {
        private readonly ConcurrentDictionary<Guid, string> _onlineUsers = new();
        private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();
        private readonly ConcurrentDictionary<Guid, string> _connections = new();

        public void Add(Guid userId, string connectionId)
        {
            _userConnections.AddOrUpdate(
                userId,
                _ => new HashSet<string> { connectionId },
                (_, list) =>
                {
                    list.Add(connectionId);
                    return list;
                }
            );
        }

        public void Remove(Guid userId, string connectionId)
        {
            if (_userConnections.TryGetValue(userId, out var list))
            {
                list.Remove(connectionId);
                if (list.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }
        }


        public bool IsOnline(Guid userId) => _connections.ContainsKey(userId);

        public List<Guid> GetOnlineUsers(List<Guid> memberIds) =>
            memberIds.Where(IsOnline).ToList();

        public List<Guid> GetAllOnlineUsers()
        {
            return _userConnections.Keys.ToList();
        }

    }
}
