using HtmlAgilityPack;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Models.DTOs;
using System.Net;

namespace SocialMediaApp.Server.CosmosDb
{
    public class CosmosDbService(CosmosDbFactory cosmosDbFactory) : ICosmosDbService
    {
        private Container UserContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "UserAccounts");
        private Container RoleContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "AccountRoles");
        private Container RoleAccountContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "LinkedRoles");
        private Container PostContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "Posts");

        public async Task<UserAccount> GetUserByIdAsync(string userId)
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
            var user = await GetUserByIdAsync(follower.Id);

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

            var users = await GetUsersFromFeedIterator(parameterizedQuery);

            return users;
        }

        public async Task<List<UserAccount>> GetUserFollowingsAsync(string userId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM UserAccounts ua WHERE ARRAY_CONTAINS(ua.followers, { 'id': @UserId }, true)")
                .WithParameter("@UserId", userId);

            var users = await GetUsersFromFeedIterator(parameterizedQuery);

            return users;
        }

        public async Task<List<UserAccount>> GetUserFollowersAsync(string userId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM UserAccounts ua ARRAY_CONTAINS(ua.following, { 'id': @UserId }, true)")
                .WithParameter("@UserId", userId);

            var users = await GetUsersFromFeedIterator(parameterizedQuery);

            return users;
        }

        public async Task<List<UserAccount>> GetUsersFromFeedIterator(QueryDefinition parameterizedQuery)
        {
            try
            {
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
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<UserAccount> UpdateUserAsync(Author user)
        {
            try
            {
                var response = await UserContainer.PatchItemAsync<UserAccount>(
                    user.Id,
                    new PartitionKey("User"),
                    patchOperations: new[]
                    {
                        PatchOperation.Replace("/description", user.Description),
                        PatchOperation.Replace("/displayName", user.DisplayName),
                        PatchOperation.Replace("/userName", user.UserName),
                        PatchOperation.Replace("/changeFeed", true)
                    });

                return response.Resource;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.Message);
                return null;
            }
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

        public async Task<List<PostDTO>> GetAllPostsAsync()
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts p WHERE p.parentPost = null ORDER BY p.createdAt DESC");

            var posts = await GetPostsFromFeedIteratorAsync(parameterizedQuery);

            foreach (var post in posts)
            {
                var quotedPosts = await GetPostQuotesAsync(post.Id);
                post.Quotes = quotedPosts;
                var postReplies = await GetPostRepliesAsync(post.Id);
                post.Replies = postReplies;
            }

            return posts;
        }

        public async Task<List<PostDTO>> GetPostQuotesAsync(string postId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts p WHERE p.quotedPost.id = @PostId ORDER BY p.createdAt DESC")
                .WithParameter("@PostId", postId);

            var posts = await GetPostsFromFeedIteratorAsync(parameterizedQuery);

            return posts;
        }

        public async Task<List<PostDTO>> GetPostRepliesAsync(string postId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts p WHERE p.parentPost.id = @PostId ORDER BY p.createdAt DESC")
                .WithParameter("@PostId", postId);

            var posts = await GetPostsFromFeedIteratorAsync(parameterizedQuery);

            return posts;
        }

        public async Task<List<PostDTO>> GetPostsFromFeedIteratorAsync(QueryDefinition parameterizedQuery)
        {
            using FeedIterator<Post> feedIterator = PostContainer.GetItemQueryIterator<Post>(
                queryDefinition: parameterizedQuery);

            List<PostDTO> posts = [];

            while (feedIterator.HasMoreResults)
            {
                foreach (Post post in await feedIterator.ReadNextAsync())
                {
                    var author = await GetUserByIdAsync(post.Author.Id);
                    var postDto = post.ToPostDTO(author);
                    posts.Add(postDto);
                }
            }
            return posts;
        }

        public async Task<List<PostDTO>> GetUserPostsAsync(string userId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts p WHERE p.author.id = @UserId AND p.parentPost = null ORDER BY p.createdAt DESC")
                .WithParameter("@UserId", userId);

            var posts = await GetPostsFromFeedIteratorAsync(parameterizedQuery);

            foreach (var post in posts)
            {
                var quotedPosts = await GetPostQuotesAsync(post.Id);
                post.Quotes = quotedPosts;
                var postReplies = await GetPostRepliesAsync(post.Id);
                post.Replies = postReplies;
            }

            return posts;
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            post.Id ??= Guid.CreateVersion7().ToString();
            post.CreatedAt = DateTimeOffset.UtcNow;
            post.AccountsLiked = [];
            post.AccountsReposted = [];
            post.PreviousVersions ??= [];
            post.Labels ??= [];
            post.ReplyIds ??= [];
            post.Replies ??= [];

            if (post.Embed.EmbedType == Enums.EmbedType.ExternalLink)
            {
                HttpClient client = new HttpClient();
                string html = await client.GetStringAsync(post.Embed.ExternalLink.ExternalLinkUri);
                HtmlDocument doc = new();
                doc.LoadHtml(html);

                var title = doc.DocumentNode.SelectSingleNode("//title").InnerText;
                var metaDescription = doc.DocumentNode.SelectSingleNode("//meta[@name='description']");
                if (metaDescription != null)
                {
                    string desc = metaDescription.Attributes["content"].Value;
                    post.Embed.ExternalLink.ExternalLinkDescription = desc;
                }
                var metaImage = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");

                if (metaImage != null)
                {
                    string thumbnailUrl = metaImage.Attributes["content"].Value;
                    post.Embed.ExternalLink.ExternalLinkThumbnail = thumbnailUrl;
                }

                post.Embed.ExternalLink.ExternalLinkTitle = title;
            }

            if (post.ParentPost is not null)
            {
                var parentPost = await GetPostByIdAsync(post.ParentPost.Id!, post.ParentPost.Author.Id);
                parentPost.ReplyCount++;
                post.ParentPost.ReplyCount = parentPost.ReplyCount;
                post.ParentPost.ReplyIds.Add(post.Id);

                await PostContainer.PatchItemAsync<Post>(
                    parentPost.Id,
                    new PartitionKey(parentPost.PartitionKey),
                    patchOperations: new[]
                    {
                        PatchOperation.Replace("/replyCount", parentPost.ReplyCount),
                        PatchOperation.Add("/replyIds/0", post.Id)
                    });
            }

            if (post.QuotedPost is not null)
            {
                var quotedPost = await GetPostByIdAsync(post.QuotedPost.Id!, post.QuotedPost.Author.Id);
                quotedPost.QuoteCount++;
                post.QuotedPost.QuoteCount = quotedPost.QuoteCount;

                await PostContainer.PatchItemAsync<Post>(
                    quotedPost.Id,
                    new PartitionKey(quotedPost.PartitionKey),
                    patchOperations: new[]
                    {
                        PatchOperation.Replace("/quoteCount", quotedPost.QuoteCount),
                    });
            }

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
            var user = await GetUserByIdAsync(userId);
            var post = await GetPostByIdAsync(id, postUserId);

            if (!user.LikedPosts.Any(p => p.Id == id))
            {
                post.LikeCount++;
                user.LikedPosts.Add(post);
                post.AccountsLiked.Add(new Author() { Id = user.Id, UserName = user.UserName, DisplayName = user.DisplayName, Description = user.Description });
            }
            else
            {
                post.LikeCount--;
                user.LikedPosts.RemoveAll(p => p.Id == post.Id);
                post.AccountsLiked.RemoveAll(a => a.Id == user.Id);
            }

            var response = await PostContainer.PatchItemAsync<Post>(
                id,
                new PartitionKey(post.PartitionKey),
                patchOperations: new[]
                {
                    PatchOperation.Replace("/likeCount", post.LikeCount),
                    PatchOperation.Replace("/accountsLiked", post.AccountsLiked)
                }
            );

            var userResponse = await UserContainer.PatchItemAsync<UserAccount>(
                userId,
                new PartitionKey($"User"),
                patchOperations: new[]
                {
                    PatchOperation.Replace("/likedPosts", user.LikedPosts)
                }
            );

            var userDto = userResponse.Resource.ToUserDTO();

            foreach (var likedPost in user.LikedPosts)
            {
                var author = await GetUserByIdAsync(likedPost.Author.Id);
                userDto.LikedPosts.Add(likedPost.ToPostDTO(author));
            }

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
            var user = await GetUserByIdAsync(userId);
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

        public async Task<Post> UpdatePostTextAsync(string postId, string userId, string text)
        {
            var post = await GetPostByIdAsync(postId, userId);

            var response = await PostContainer.PatchItemAsync<Post>(
                postId,
                new PartitionKey(post.PartitionKey),
                patchOperations: new[]
                {
                    PatchOperation.Replace("/text", text),
                    PatchOperation.Replace("/likeCount", 0),
                    PatchOperation.Replace("/repostCount", 0),
                    PatchOperation.Replace("/quoteCount", 0),
                    PatchOperation.Replace("/replyCount", 0),
                    PatchOperation.Replace("/bookmarkCount", 0),
                    PatchOperation.Replace("/createdAt", DateTimeOffset.UtcNow),
                    PatchOperation.Replace("/accountsLiked", new List<Author>()),
                    PatchOperation.Replace("/accountsReposted", new List<Author>()),
                    PatchOperation.Replace("/replyIds", new List<string>()),
                    PatchOperation.Add("/previousVersions/0", post)
                }
            );

            return response.Resource;
        }
    }
}
