using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.CosmosDb
{
    public interface ICosmosDbService
    {
        Task<UserAccount> AddAsync(UserAccount account);
        Task<UserAccount> GetByEmailAsync(string email);
        Task<UserAccount> GetUserByUserNameAsync(string userName);
        Task<UserDTO> GetUserAsync(string userId);

        Task<AccountRole> GetRoleByNameAsync(string roleName);
        Task<AccountRole> AddRoleAsync(string roleName);
        Task<string> GetUserRoleAsync(UserAccount account);

        Task<Post> CreatePostAsync(Post post);
        Task<List<Post>> GetUserPostsAsync(string userId);
        Task<List<Post>> GetAllPostsAsync();
    }
}
