using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using SocialMediaApp.Server.Models;
using System.Net;

namespace SocialMediaApp.Server.CosmosDb
{
    public class CosmosDbService(CosmosDbFactory cosmosDbFactory) : ICosmosDbService
    {
        private Container UserContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "UserAccounts");
        private Container RoleContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "AccountRoles");
        private Container RoleAccountContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "LinkedRoles");
        private Container PostContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "Posts");

        public async Task<UserAccount> GetUserAsync(string userId)
        {
            var userResponse = await UserContainer.ReadItemAsync<UserAccount>(userId, new PartitionKey("User"));

            return userResponse.Resource;
        }

        public async Task<UserAccount> AddAsync(UserAccount account)
        {
            var existingEmail = await GetByEmailAsync(account.Email);
            var existingUserName = await GetUserByUserNameAsync(account.UserName);

            if (existingEmail is null && existingUserName is null)
            {
                var role = await GetRoleByNameAsync("User");

                var response = await UserContainer.CreateItemAsync(account, new PartitionKey(account.PartitionKey));

                AccountRoleLinked linkedRole = new() { RoleId = role.Id, AccountId = response.Resource.Id };

                var addRoleResponse = await RoleAccountContainer.CreateItemAsync(linkedRole, new PartitionKey(linkedRole.RoleId));

                return response.Resource;
            }

            return null;
        }

        public async Task<UserAccount> GetUserByEmailOrUserNameAsync(string emailOrUserName)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM UserAccounts ua WHERE ua.email = @EmailOrUserName OR ua.userName = @EmailOrUserName")
                .WithParameter("@EmailOrUserName", emailOrUserName);

            var user = await GetUserFromFeedIterator(parameterizedQuery);

            return user;
        }

        public async Task<UserAccount> GetByEmailAsync(string email)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM UserAccounts ua WHERE ua.email = @Email")
                .WithParameter("@Email", email);

            var user = await GetUserFromFeedIterator(parameterizedQuery);

            return user;
        }

        public async Task<UserAccount> GetUserByUserNameAsync(string userName)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM UserAccounts ua WHERE ua.userName = @UserName")
                .WithParameter("@UserName", userName);

            var user = await GetUserFromFeedIterator(parameterizedQuery);

            return user;
        }

        public async Task<UserAccount> GetUserFromFeedIterator(QueryDefinition parameterizedQuery)
        {
            using FeedIterator<UserAccount> feedIterator = UserContainer.GetItemQueryIterator<UserAccount>(
                queryDefinition: parameterizedQuery);

            List<UserAccount> existingAccounts = [];

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<UserAccount> users = await feedIterator.ReadNextAsync();

                foreach (UserAccount user in users)
                {
                    existingAccounts.Add(user);
                }
            }

            if (existingAccounts.Count > 0)
                return existingAccounts.First();
            else
                return null;
        }

        public async Task<object> FollowUserAsync(string userName, Author follower)
        {
            var user = await GetUserAsync(follower.Id);

            var followee = await GetUserByUserNameAsync(userName);

            if (user is null || followee is null) return null;

            if (!followee.Followers.Any(f => f.UserName == follower.UserName))
            {
                if (followee.AccountSettings.IsPrivate)
                    followee.FollowRequests.Add(follower);
                else
                {
                    followee.Followers.Add(follower);
                    user.Following.Add(new Author() { Id = followee.Id, UserName = followee.UserName, DisplayName = followee.DisplayName, Description = followee.Description });
                }
            }
            else
            {
                if (followee.AccountSettings.IsPrivate && followee.FollowRequests.Any(f => f.Id == follower.Id))
                    followee.FollowRequests.RemoveAll(f => f.Id == follower.Id);
                else
                {
                    followee.Followers.Remove(followee.Followers.Find(f => f.Id == follower.Id)!);
                    user.Following.Remove(user.Following.Find(f => f.Id == followee.Id)!);
                }
            }

            var updatedUser = await FollowPatchOperation(followee, user);

            return updatedUser;
        }

        public async Task<object> ConfirmFollowAsync(string userName, Author follower)
        {
            var user = await GetUserByUserNameAsync(userName);

            var followee = await GetUserByUserNameAsync(userName);

            if (user is null || followee is null) return null;

            followee.Followers.Add(follower);
            followee.FollowRequests.RemoveAll(f => f.Id == follower.Id);
            user.Following.Add(new Author() { Id = followee.Id, UserName = followee.UserName, DisplayName = followee.DisplayName, Description = followee.Description });

            var updatedUser = await FollowPatchOperation(followee, user);

            return updatedUser;
        }

        public async Task<object> FollowPatchOperation(UserAccount followee, UserAccount user)
        {
            var followeeResponse = await UserContainer.PatchItemAsync<UserAccount>(
                followee.Id,
                new PartitionKey("User"),
                patchOperations: new[]
                {
                    PatchOperation.Replace("/followers", followee.Followers),
                    PatchOperation.Replace("/followRequests", followee.FollowRequests)
                }
            );

            var followeeDto = followeeResponse.Resource.ToUserDTO();

            var userResponse = await UserContainer.PatchItemAsync<UserAccount>(
                user.Id,
                new PartitionKey("User"),
                patchOperations: new[]
                {
                    PatchOperation.Replace("/following", user.Following)
                }
            );

            var userDto = userResponse.Resource.ToUserDTO();

            return new { followee = followeeDto, user = userDto };
        }

        public async Task<List<UserAccount>> MatchingUsersAsync(string searchTerm)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM UserAccounts ua WHERE CONTAINS(LOWER(ua.userName), @SearchTerm) OR CONTAINS(LOWER(ua.displayName), @SearchTerm)")
                .WithParameter("@SearchTerm", searchTerm.ToLower());

            using FeedIterator<UserAccount> feedIterator = UserContainer.GetItemQueryIterator<UserAccount>(
                queryDefinition: parameterizedQuery);

            List<UserAccount> matchingUsers = [];

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<UserAccount> users = await feedIterator.ReadNextAsync();
                foreach (var user in users)
                {
                    matchingUsers.Add(user);
                }
            }
            return matchingUsers;
        }

        public async Task<AccountRole> AddRoleAsync(string roleName)
        {
            var roleToCreate = new AccountRole() { RoleName = roleName };

            var response = await RoleContainer.CreateItemAsync(roleToCreate, new PartitionKey(roleToCreate.PartitionKey));

            return response.Resource;
        }

        public async Task<AccountRole> GetRoleByNameAsync(string roleName)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM UserRoles ur WHERE ur.roleName = @Name")
                .WithParameter("@Name", roleName);

            using FeedIterator<AccountRole> feedIterator = RoleContainer.GetItemQueryIterator<AccountRole>(
                queryDefinition: parameterizedQuery);

            List<AccountRole> existingRoles = [];

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<AccountRole> roles = await feedIterator.ReadNextAsync();

                foreach (var role in roles)
                {
                    existingRoles.Add(role);
                }
            }

            if (existingRoles.Count > 0)
                return existingRoles.First();
            else
                return null;
        }

        public async Task<string> GetUserRoleAsync(UserAccount account)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM LinkedRoles lr WHERE lr.accountId = @AccountId")
                .WithParameter("@AccountId", account.Id);

            using FeedIterator<AccountRoleLinked> feedIterator = RoleAccountContainer.GetItemQueryIterator<AccountRoleLinked>(
                queryDefinition: parameterizedQuery);

            List<AccountRoleLinked> existingRoles = [];

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<AccountRoleLinked> roles = await feedIterator.ReadNextAsync();

                foreach (var linkedRole in roles)
                {
                    existingRoles.Add(linkedRole);
                }
            }

            if (existingRoles.Count > 0)
            {
                var userRole = existingRoles.First();

                string id = userRole.RoleId;

                var response = await RoleContainer.ReadItemAsync<AccountRole>(id, new PartitionKey("AccountRole"));

                return response.Resource.RoleName;
            }
            else return null;
        }

        public async Task<List<Post>> GetAllPostsAsync()
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts");

            using FeedIterator<Post> feedIterator = PostContainer.GetItemQueryIterator<Post>(
                queryDefinition: parameterizedQuery);

            List<Post> allPosts = [];

            while (feedIterator.HasMoreResults)
            {
                //FeedResponse<Post> posts = await feedIterator.ReadNextAsync();

                foreach (Post post in await feedIterator.ReadNextAsync())
                {
                    allPosts.Add(post);
                }
            }

            return allPosts;
        }

        public async Task<List<Post>> GetUserPostsAsync(string userId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts p WHERE p.author.id = @UserId ORDER BY p.createdAt DESC")
                .WithParameter("@UserId", userId);

            using FeedIterator<Post> feedIterator = PostContainer.GetItemQueryIterator<Post>(
                queryDefinition: parameterizedQuery);

            List<Post> userPosts = [];

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<Post> posts = await feedIterator.ReadNextAsync();

                foreach (var userPost in posts)
                {
                    userPosts.Add(userPost);
                }
            }

            return userPosts;
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            var response = await PostContainer.CreateItemAsync(post, new PartitionKey(post.PartitionKey));

            return response.Resource;
        }

        public async Task<bool> DeletePostAsync(string postId, string userId)
        {
            var response = await PostContainer.DeleteItemAsync<Post>(postId, new PartitionKey($"Post-{userId}"));

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                var usersToUpdate = new List<UserAccount>();

                using (FeedIterator<UserAccount> feedIterator = UserContainer.GetItemLinqQueryable<UserAccount>()
                    .Where(u => u.RepostedPosts.Any(rp => rp.Id == postId)
                            || u.LikedPosts.Any(lp => lp.Id == postId)
                            || u.Bookmarks.Any(b => b.Id == postId))
                    .ToFeedIterator())
                {
                    while (feedIterator.HasMoreResults)
                    {
                        foreach (UserAccount user in await feedIterator.ReadNextAsync())
                        {
                            user.RepostedPosts.RemoveAll(rp => rp.Id == postId);
                            user.LikedPosts.RemoveAll(lp => lp.Id == postId);
                            user.Bookmarks.RemoveAll(b => b.Id == postId);
                            usersToUpdate.Add(user);
                        }
                    }
                }
                foreach (var user in usersToUpdate)
                {
                    await UserContainer.PatchItemAsync<UserAccount>(
                        user.Id,
                        new PartitionKey("User"),
                        patchOperations: new[]
                        {
                            PatchOperation.Replace("/repostedPosts", user.RepostedPosts),
                            PatchOperation.Replace("/likedPosts", user.LikedPosts),
                            PatchOperation.Replace("/bookmarks", user.Bookmarks)
                        }
                    );
                }
            }

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<Post> GetPostByIdAsync(string id, string userId)
        {
            try
            {
                var response = await PostContainer.ReadItemAsync<Post>(id, new PartitionKey($"Post-{userId}"));

                return response.Resource;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<object> LikePostAsync(string id, string userId, string postUserId)
        {
            var user = await GetUserAsync(userId);
            var userLikedPosts = user.LikedPosts;
            var post = await GetPostByIdAsync(id, postUserId);
            var accountsLiked = post.AccountsLiked;

            if (!user.LikedPosts.Any(p => p.Id == id))
            {
                post.LikeCount++;
                userLikedPosts.Add(post);
                accountsLiked.Add(new Author() { Id = user.Id, UserName = user.UserName, DisplayName = user.DisplayName });
            }
            else
            {
                post.LikeCount--;
                userLikedPosts.RemoveAll(p => p.Id == post.Id);
                accountsLiked.RemoveAll(a => a.Id == user.Id);
            }

            var response = await PostContainer.PatchItemAsync<Post>(
                id,
                new PartitionKey($"Post-{postUserId}"),
                patchOperations: new[]
                {
                    PatchOperation.Replace("/likeCount", post.LikeCount),
                    PatchOperation.Replace("/accountsLiked", accountsLiked)
                }
            );

            var userResponse = await UserContainer.PatchItemAsync<UserAccount>(
                userId,
                new PartitionKey($"User"),
                patchOperations: new[]
                {
                    PatchOperation.Replace("/likedPosts", userLikedPosts)
                }
            );

            var userDto = userResponse.Resource.ToUserDTO();

            return new { post = response.Resource, user = userDto };
        }

        public async Task<List<Post>> GetMatchingPostsAsync(string searchTerm)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts p WHERE CONTAINS(LOWER(p.text), @SearchTerm)")
                .WithParameter("@SearchTerm", searchTerm.ToLower());

            using FeedIterator<Post> feedIterator = PostContainer.GetItemQueryIterator<Post>(
                queryDefinition: parameterizedQuery);

            List<Post> matchingPosts = [];

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<Post> posts = await feedIterator.ReadNextAsync();
                foreach (var post in posts)
                {
                    matchingPosts.Add(post);
                }
            }
            return matchingPosts;
        }

        public async Task<UserAccount> BookmarkPost(string postId, string postUserId, string userId)
        {
            var user = await GetUserAsync(userId);
            var post = await GetPostByIdAsync(postId, userId);

            if (user.Bookmarks.Any(b => b.Id == postId))
            {
                user.Bookmarks.RemoveAll(b => b.Id == postId);
            }
            else
            {
                user.Bookmarks.Add(post);
            }

            var response = await UserContainer.PatchItemAsync<UserAccount>(
                userId,
                new PartitionKey("User"),
                patchOperations: new[]
                {
                    PatchOperation.Replace("/bookmarks", user.Bookmarks)
                }
            );

            await PostContainer.PatchItemAsync<Post>(
                postId,
                new PartitionKey($"Post-{postUserId}"),
                patchOperations: new[]
                {
                    PatchOperation.Replace("/bookmarkCount", post.BookmarkCount+1)
                }
            );

            return response.Resource;
        }
    }
}
