using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using SocialMediaApp.Server.CosmosDb;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Models.DTOs;

namespace SocialMediaApp.Server.Services
{
    public class ImageService(IConfiguration configuration, CosmosDbFactory cosmosDbFactory)
    {
        string ConnectionString = configuration.GetValue<string>("BlobStorage:DefaultConnection")!;
        string ProfileContainerName = configuration.GetValue<string>("BlobStorage:ProfileContainerName")!;
        string PostContainerName = configuration.GetValue<string>("BlobStorage:PostContainerName")!;
        private Container UserContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "UserAccounts");
        private Container PostContainer => cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "Posts");

        public async Task<string> UploadImageAsync(IFormFile file, string fileName, string userId)
        {
            var blobServiceClient = new BlobServiceClient(ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(ProfileContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(file.OpenReadStream(), true);

            var userResponse = await UserContainer.ReadItemAsync<UserAccount>(userId, new PartitionKey("User"));
            var user = userResponse.Resource;

            if (!user.ProfilePicture.Contains(userId))
            {
                await UserContainer.PatchItemAsync<UserAccount>(
                    userId,
                    new PartitionKey("User"),
                    patchOperations: new[]
                    {
                        PatchOperation.Replace("/profilePicture", blobClient.Uri.ToString()),
                        PatchOperation.Replace("/changeFeed", true)
                    });
            }

            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadPostImageAsync(IFormFile file, string fileName, string postId, string userId)
        {
            var blobServiceClient = new BlobServiceClient(ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(PostContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(file.OpenReadStream(), true);

            return blobClient.Uri.ToString();
        }

        public async Task<Post> UpdatePostMediaAsync(PostDTO post)
        {
            var response = await PostContainer.PatchItemAsync<Post>(
                post.Id,
                new PartitionKey(post.PartitionKey),
                patchOperations: new[]
                {
                    PatchOperation.Replace("/embed/images", post.Embed.Images)
                });

            return response.Resource;
        }
    }
}
