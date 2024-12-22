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

        public async Task<Post> GetPostByIdAsync(string id, string userId)
        {
            var post = await cosmosDbService.GetPostByIdAsync(id, userId);

            return post;
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            var createdPost = await cosmosDbService.CreatePostAsync(post);

            return createdPost;
        }

        public async Task<bool> DeletePostAsync(string postId, string userId)
        {
            var postDeleted = await cosmosDbService.DeletePostAsync(postId, userId);

            return postDeleted;
        }

        public async Task<object> LikePostAsync(string id, string authorId, string userId)
        {
            var post = await cosmosDbService.GetPostByIdAsync(id, authorId);

            if (post is null) return new { Message = "There was an error trying to fetch the post." };

            var updatedPostUser = await cosmosDbService.LikePostAsync(id, userId, authorId);

            return updatedPostUser;
        }

        public async Task<List<Post>> GetMatchingPostsAsync(string searchTerm)
        {
            var posts = await cosmosDbService.GetMatchingPostsAsync(searchTerm);

            return posts;
        }

        public async Task<UserAccount> BookmarkPost(string postId, string postUserId, string userId)
        {
            var user = await cosmosDbService.BookmarkPost(postId, postUserId, userId);

            return user;
        }
    }
}
