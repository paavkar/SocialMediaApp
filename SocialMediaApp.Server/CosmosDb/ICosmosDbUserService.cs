using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.CosmosDb
{
    public interface ICosmosDbUserService
    {
        Task<UserAccount> AddAsync(UserAccount account);
        Task<UserAccount> GetByEmailAsync(string email);
    }
}
