using SocialMediaApp.Server.CosmosDb;
using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.Services
{
    public class RoleManager(ICosmosDbUserService cosmosDbUserService)
    {
        public async Task<bool> CreateRoleIfNotExistAsync(string roleName)
        {
            var existingRole = await cosmosDbUserService.GetRoleByNameAsync(roleName);
            AccountRole createdRole = new();

            if (existingRole is null)
            {
                createdRole = await cosmosDbUserService.AddRoleAsync(roleName);
            }

            return existingRole is not null || createdRole is not null;
        }

        public async Task<string> GetUserRoleAsync(UserAccount account)
        {
            var role = await cosmosDbUserService.GetUserRoleAsync(account);

            return role;
        }
    }
}
