using SocialMediaApp.Server.CosmosDb;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Models.DTOs;

namespace SocialMediaApp.Server.Services
{
    public class PostsService(ICosmosDbService cosmosDbService)
    {
        public async Task<List<PostDTO>> GetAllPostsAsync()
        {
            var posts = await cosmosDbService.GetAllPostsAsync();

            return posts;
        }

        public async Task<List<PostDTO>> GetUserPostsAsync(string userId)
        {
            var posts = await cosmosDbService.GetUserPostsAsync(userId);

            return posts;
        }

        public async Task<PostDTO> GetPostByIdAsync(string id, string userId)
        {
            var post = await cosmosDbService.GetPostByIdAsync(id, userId);

            var postDto = await GetPostDTOAsync(post);

            return postDto;
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

        public async Task<UserDTO> BookmarkPost(string postId, string postUserId, string userId)
        {
            var user = await cosmosDbService.BookmarkPost(postId, postUserId, userId);

            var userDto = user.ToUserDTO();

            foreach (var bookmark in user.Bookmarks)
            {
                var author = await cosmosDbService.GetUserByIdAsync(bookmark.Author.Id);
                userDto.Bookmarks.Add(bookmark.ToPostDTO(author));
            }

            return userDto;
        }

        public async Task<PostDTO> UpdatePostTextAsync(string postId, string userId, string text)
        {
            var post = await cosmosDbService.UpdatePostTextAsync(postId, userId, text);

            var postDto = await GetPostDTOAsync(post);

            return postDto;
        }

        public async Task<PostDTO> GetPostDTOAsync(Post post)
        {
            if (post is null) return null;

            var author = await cosmosDbService.GetUserByIdAsync(post.Author.Id);

            var postDto = post.ToPostDTO(author);

            var quotedPosts = await cosmosDbService.GetPostQuotesAsync(post.Id!);
            postDto.Quotes = quotedPosts;

            var postReplies = await cosmosDbService.GetPostRepliesAsync(post.Id!);

            foreach (var postReply in postReplies)
            {
                if (postDto.ReplyIds.Any(p => p == postReply.Id))
                {
                    postDto.Replies.Add(postReply);
                }

                if (postDto.PreviousVersions.Count > 0)
                {
                    foreach (var previousVersion in postDto.PreviousVersions)
                    {
                        if (previousVersion.ReplyIds.Any(p => p == postReply.Id))
                        {
                            previousVersion.Replies.Add(postReply);
                        }
                    }
                }
            }

            var reply = postDto;
            PostDTO authorReply = null;

            if (postReplies.Any(r => r.ParentPost!.Author.Id == post.Author.Id))
            {
                postDto.AuthorThread ??= [];
                authorReply = postReplies.First(r => r.ParentPost!.Id == post.Id);
            }

            while (reply.ParentPost != null)
            {
                postDto.Thread ??= [];
                var parent = await cosmosDbService.GetPostByIdAsync(reply.ParentPost.Id!, reply.ParentPost.Author.Id);
                var parentAuthor = await cosmosDbService.GetUserByIdAsync(parent.Author.Id);

                postDto.Thread.Add(parent.ToPostDTO(parentAuthor));

                reply = parent.ToPostDTO(parentAuthor);
            }

            while (authorReply != null)
            {
                postDto.AuthorThread.Add(authorReply);
                var replyReplies = await cosmosDbService.GetPostRepliesAsync(authorReply.Id!);

                if (replyReplies.Any(r => r.ParentPost!.Id == authorReply.Id))
                    authorReply = replyReplies.First(r => r.ParentPost!.Id == authorReply.Id);
                else
                    authorReply = null;
            }

            return postDto;
        }
    }
}
