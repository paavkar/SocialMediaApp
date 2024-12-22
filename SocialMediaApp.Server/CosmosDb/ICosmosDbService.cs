﻿using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.CosmosDb
{
    public interface ICosmosDbService
    {
        Task<UserAccount> AddAsync(UserAccount account);
        Task<UserAccount> GetUserByEmailOrUserNameAsync(string emailOrUserName);
        Task<UserAccount> GetByEmailAsync(string email);
        Task<UserAccount> GetUserByUserNameAsync(string userName);
        Task<UserAccount> GetUserAsync(string userId);
        Task<UserAccount> FollowUserAsync(string userName, Author follower);
        Task<UserAccount> ConfirmFollowAsync(string userName, Author follower);
        Task<List<UserAccount>> MatchingUsersAsync(string searchTerm);

        Task<AccountRole> GetRoleByNameAsync(string roleName);
        Task<AccountRole> AddRoleAsync(string roleName);
        Task<string> GetUserRoleAsync(UserAccount account);

        Task<Post> CreatePostAsync(Post post);
        Task<bool> DeletePostAsync(string postId, string userId);
        Task<List<Post>> GetUserPostsAsync(string userId);
        Task<List<Post>> GetAllPostsAsync();
        Task<Post> GetPostByIdAsync(string id, string userId);
        Task<object> LikePostAsync(string id, string userId, string postUserId);
        Task<List<Post>> GetMatchingPostsAsync(string searchTerm);
        Task<UserAccount> BookmarkPost(string postId, string postUserId, string userId);
    }
}
