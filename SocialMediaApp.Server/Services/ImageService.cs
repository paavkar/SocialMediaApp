using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using SocialMediaApp.Server.CosmosDb;
using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.Services
{
    public class ImageService(IConfiguration configuration, CosmosDbFactory cosmosDbFactory)
    {
        string ConnectionString = configuration.GetValue<string>("BlobStorage:DefaultConnection")!;
        string ContainerName = configuration.GetValue<string>("BlobStorage:ContainerName")!;
        private Container UserContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "UserAccounts");

        public async Task<string> UploadImageAsync(IFormFile file, string fileName, string userId)
        {
            var blobServiceClient = new BlobServiceClient(ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(file.OpenReadStream(), true);

            await UserContainer.PatchItemAsync<UserAccount>(
                userId,
                new PartitionKey("User"),
                patchOperations: new[]
                {
                    PatchOperation.Replace("/profilePicture", blobClient.Uri.ToString())
                });

            return blobClient.Uri.ToString();
        }
    }
}
