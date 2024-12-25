using Microsoft.Azure.Cosmos;

namespace SocialMediaApp.Server.CosmosDb
{
    public class CosmosDbFactory
    {
        public CosmosClient CosmosClient { get; set; }
        public string DatabaseName { get; set; }

        private Container _container;
        private Container _roleContainer;
        private Container _roleAccountContainer;
        private Container _postContainer;

        public CosmosDbFactory(IConfiguration configuration)
        {
            CosmosDbSettings settings = configuration
                                             .GetSection(CosmosDbSettings.Section)
                                             .Get<CosmosDbSettings>()
                                         ?? throw new ArgumentNullException(nameof(CosmosDbSettings));

            var serializationOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            };

            CosmosClient = new CosmosClient(settings.ConnectionString, new CosmosClientOptions()
            {
                SerializerOptions = serializationOptions
            });

            DatabaseName = settings.DatabaseName ?? throw new ArgumentNullException(nameof(CosmosDbSettings));
        }

        public async Task InitializeDatabase()
        {
            await CosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseName);

            var database = CosmosClient.GetDatabase(DatabaseName);

            await database.CreateContainerIfNotExistsAsync(id: "UserAccounts", partitionKeyPath: "/partitionKey");
            await database.CreateContainerIfNotExistsAsync(id: "AccountRoles", partitionKeyPath: "/partitionKey");
            await database.CreateContainerIfNotExistsAsync(id: "LinkedRoles", partitionKeyPath: "/roleId");
            await database.CreateContainerIfNotExistsAsync(id: "Posts", partitionKeyPath: "/partitionKey");
            await database.CreateContainerIfNotExistsAsync(id: "leaseContainer", partitionKeyPath: "/partitionKey");
        }
    }
}
