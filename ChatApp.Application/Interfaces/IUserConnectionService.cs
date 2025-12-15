using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces
{
    public interface IUserConnectionService
    {
        void Add(Guid userId, string connectionId);
        void Remove(Guid userId, string connectionId);
        bool IsOnline(Guid userId);
        List<Guid> GetOnlineUsers(List<Guid> memberIds);
        List<Guid> GetAllOnlineUsers();
    }

}
