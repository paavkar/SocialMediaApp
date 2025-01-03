﻿using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Models.DTOs;

namespace SocialMediaApp.Server.CosmosDb
{
    public interface ICosmosDbService
    {
        Task<UserAccount> AddAsync(UserAccount account);
        Task<UserAccount> GetUserByEmailOrUserNameAsync(string emailOrUserName);
        Task<UserAccount> GetByEmailAsync(string email);
        Task<UserAccount> GetUserByUserNameAsync(string userName);
        Task<UserAccount> GetUserByIdAsync(string userId);
        Task<object> FollowUserAsync(string userName, Author follower);
        Task<object> ConfirmFollowAsync(string userName, Author follower);
        Task<List<UserAccount>> GetUserFollowingsAsync(string userId);
        Task<List<UserAccount>> GetUserFollowersAsync(string userId);
        Task<List<UserAccount>> MatchingUsersAsync(string searchTerm);
        Task<UserAccount> UpdateUserAsync(Author user);

        Task<AccountRole> GetRoleByNameAsync(string roleName);
        Task<AccountRole> AddRoleAsync(string roleName);
        Task<string> GetUserRoleAsync(UserAccount account);

        Task<Post> CreatePostAsync(Post post);
        Task<bool> DeletePostAsync(string postId, string userId);
        Task<List<PostDTO>> GetUserPostsAsync(string userId);
        Task<List<PostDTO>> GetAllPostsAsync();
        Task<List<PostDTO>> GetPostQuotesAsync(string postId);
        Task<List<PostDTO>> GetPostRepliesAsync(string postId);
        Task<Post> GetPostByIdAsync(string id, string userId);
        Task<object> LikePostAsync(string id, string userId, string postUserId);
        Task<List<Post>> GetMatchingPostsAsync(string searchTerm);
        Task<UserAccount> BookmarkPost(string postId, string postUserId, string userId);
        Task<Post> UpdatePostTextAsync(string postId, string userId, string text);
    }
}
