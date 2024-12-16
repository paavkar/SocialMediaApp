using Microsoft.Azure.Cosmos;

namespace SocialMediaApp.Server.CosmosDb
{
    public class CosmosDbFactory
    {
        public CosmosClient CosmosClient { get; set; }
        public string DatabaseName { get; set; }
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
    }
}
