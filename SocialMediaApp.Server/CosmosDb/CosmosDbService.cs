using Microsoft.Azure.Cosmos;
using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.CosmosDb
{
    public class CosmosDbService(CosmosDbFactory cosmosDbFactory) : ICosmosDbService
    {
        private Container _container => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "UserAccounts");
        private Container _roleContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "AccountRoles");
        private Container _roleAccountContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "LinkedRoles");
        private Container _postContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "Posts");

        public async Task<UserAccount> GetUserAsync(string userId)
        {
            var userResponse = await _container.ReadItemAsync<UserAccount>(userId, new PartitionKey("User"));

            return userResponse.Resource;
        }

        public async Task<UserAccount> AddAsync(UserAccount account)
        {
            var existingEmail = await GetByEmailAsync(account.Email);
            var existingUserName = await GetUserByUserNameAsync(account.UserName);

            if (existingEmail is null && existingUserName is null)
            {
                var role = await GetRoleByNameAsync("User");

                var response = await _container.CreateItemAsync(account, new PartitionKey(account.PartitionKey));

                AccountRoleLinked linkedRole = new() { RoleId = role.Id, AccountId = response.Resource.Id };

                var addRoleResponse = await _roleAccountContainer.CreateItemAsync(linkedRole, new PartitionKey(linkedRole.RoleId));

                return response.Resource;
            }

            return null;
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
            using FeedIterator<UserAccount> feedIterator = _container.GetItemQueryIterator<UserAccount>(
                queryDefinition: parameterizedQuery);

            List<UserAccount> existingAccounts = new();

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

        public async Task<AccountRole> AddRoleAsync(string roleName)
        {
            var roleToCreate = new AccountRole() { RoleName = roleName };

            var response = await _roleContainer.CreateItemAsync(roleToCreate, new PartitionKey(roleToCreate.PartitionKey));

            return response.Resource;
        }

        public async Task<AccountRole> GetRoleByNameAsync(string roleName)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM UserRoles ur WHERE ur.roleName = @Name")
                .WithParameter("@Name", roleName);

            using FeedIterator<AccountRole> feedIterator = _roleContainer.GetItemQueryIterator<AccountRole>(
                queryDefinition: parameterizedQuery);

            List<AccountRole> existingRoles = new();

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

            using FeedIterator<AccountRoleLinked> feedIterator = _roleAccountContainer.GetItemQueryIterator<AccountRoleLinked>(
                queryDefinition: parameterizedQuery);

            List<AccountRoleLinked> existingRoles = new();

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

                var response = await _roleContainer.ReadItemAsync<AccountRole>(id, new PartitionKey("AccountRole"));

                return response.Resource.RoleName;
            }
            else return null;
        }

        public async Task<List<Post>> GetAllPostsAsync()
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts");

            using FeedIterator<Post> feedIterator = _postContainer.GetItemQueryIterator<Post>(
                queryDefinition: parameterizedQuery);

            List<Post> allPosts = new();

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<Post> posts = await feedIterator.ReadNextAsync();

                foreach (var post in posts)
                {
                    allPosts.Add(post);
                }
            }

            return allPosts;
        }

        public async Task<List<Post>> GetUserPostsAsync(string userId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts p WHERE p.author.id = @UserId ORDER BY p.createdAt")
                .WithParameter("@UserId", userId);

            using FeedIterator<Post> feedIterator = _postContainer.GetItemQueryIterator<Post>(
                queryDefinition: parameterizedQuery);

            List<Post> userPosts = new();

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
            var response = await _postContainer.CreateItemAsync(post, new PartitionKey(post.PartitionKey));

            return response.Resource;
        }
    }
}
