using Microsoft.Azure.Cosmos;
using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.CosmosDb
{
    public class CosmosDbUserService(CosmosDbFactory cosmosDbFactory) : ICosmosDbUserService
    {
        private Container _container => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "UserAccounts");

        public async Task<UserAccount> AddAsync(UserAccount account)
        {
            var existingUser = await GetByEmailAsync(account.Email);

            if (existingUser is not UserAccount)
            {
                var response = await _container.CreateItemAsync(account, new PartitionKey(account.PartitionKey));

                return response.Resource;
            }

            return null;
        }

        public async Task<UserAccount> GetByEmailAsync(string email)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT (1) FROM UserAccounts ua WHERE ua.email = @Email")
                .WithParameter("@Email", email);

            using FeedIterator<UserAccount> feedIterator = _container.GetItemQueryIterator<UserAccount>(
                queryDefinition: parameterizedQuery);

            List<UserAccount> existingAccounts = new();

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<UserAccount> users = await feedIterator.ReadNextAsync();

                foreach (var user in users)
                {
                    existingAccounts.Add(user);
                }
            }

            if (existingAccounts.Count > 0)
                return existingAccounts.First();
            else
                return null;
        }
    }
}
