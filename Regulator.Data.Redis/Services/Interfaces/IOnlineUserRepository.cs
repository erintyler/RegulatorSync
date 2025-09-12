using Regulator.Data.Redis.Models;

namespace Regulator.Data.Redis.Services.Interfaces;

public interface IOnlineUserRepository : IRepository<string, OnlineUser>
{
    
}