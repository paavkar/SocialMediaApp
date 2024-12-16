using SocialMediaApp.Server.CosmosDb;
using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.Services
{
    public class UserManager(ICosmosDbUserService cosmosDbUserService, RoleManager roleManager)
    {
        public async Task<UserAccount> CreateUserAsync(UserAccount account)
        {
            var user = await cosmosDbUserService.AddAsync(account);

            return user;
        }

        public async Task<UserAccount> GetUserByEmailAsync(string email)
        {
            var user = await cosmosDbUserService.GetByEmailAsync(email);

            return user;
        }
    }
}
