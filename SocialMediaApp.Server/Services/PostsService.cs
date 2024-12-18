using SocialMediaApp.Server.CosmosDb;
using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.Services
{
    public class PostsService(ICosmosDbService cosmosDbService)
    {
        public async Task<List<Post>> GetAllPostsAsync()
        {
            var posts = await cosmosDbService.GetAllPostsAsync();

            return posts;
        }

        public async Task<List<Post>> GetUserPostsAsync(string userId)
        {
            var posts = await cosmosDbService.GetUserPostsAsync(userId);

            return posts;
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            var createdPost = await cosmosDbService.CreatePostAsync(post);

            return createdPost;
        }

        public async Task<object> LikePostAsync(string id, string userId)
        {
            var post = await cosmosDbService.GetPostByIdAsync(id, userId);
            int likeCount = post.LikeCount + 1;
            var updatedPostUser = await cosmosDbService.LikePostAsync(id, userId, likeCount);

            return updatedPostUser;
        }
    }
}
