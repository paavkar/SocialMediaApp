using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Services;
using System.Security.Claims;

namespace SocialMediaApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController(UserManager userManager, PostsService postsService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized("No valid token given with request.");

            var posts = await postsService.GetAllPostsAsync();

            return Ok(posts);
        }

        [HttpGet("user-posts/{userName}")]
        public async Task<IActionResult> GetUserPosts(string userName)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            var user = await userManager.GetUserByUserNameAsync(userName);

            if (user.AccountSettings.SignInRequired && String.IsNullOrEmpty(userId))
                return Unauthorized("This user requires sign-in to view their profile.");
            if (user.AccountSettings.IsPrivate && user.Followers.Any(f => f.Id == userId))
                return Unauthorized("This user's profile is private.");

            var posts = await postsService.GetUserPostsAsync(user.Id);

            List<dynamic> chronologicalPosts = [];
            Dictionary<string, dynamic> postDictionary = [];

            foreach (var post in posts)
            {
                foreach (var repost in user.RepostedPosts)
                {
                    if (repost.RepostedAt > post.CreatedAt)
                    {
                        postDictionary.Remove(repost.Id);
                        postDictionary.Add(repost.Id, repost);
                    }
                    if (!postDictionary.ContainsKey(post.Id))
                        postDictionary.Add(post.Id, post);
                }
                if (!postDictionary.ContainsKey(post.Id))
                    postDictionary.Add(post.Id, post);
            }

            foreach (var post in postDictionary)
            {
                chronologicalPosts.Add(post.Value);
            }

            return Ok(chronologicalPosts);
        }

        [HttpGet("post/{userName}/{postId}")]
        public async Task<IActionResult> GetPost(string userName, string postId)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized("No valid token given with request.");

            var user = await userManager.GetUserByUserNameAsync(userName);
            var post = await postsService.GetPostByIdAsync(postId, user.Id);

            return Ok(post);
        }

        [HttpPost("post")]
        public async Task<IActionResult> Post([FromBody] Post post)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized("No valid token given with request.");

            var user = await userManager.GetUserByIdAsync(userId);

            post.PartitionKey = $"Post-{userId}";

            var createdPost = await postsService.CreatePostAsync(post);
            var userPosts = await postsService.GetUserPostsAsync(userId);

            return CreatedAtAction(nameof(Post), new { createdPost, userPosts });
        }

        [HttpDelete("delete-post/{postId}")]
        public async Task<IActionResult> DeletePost(string postId, [FromBody] Author postAuthor)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized("No valid token given with request.");
            if (userId != postAuthor.Id)
                return Unauthorized("You are not authorized to delete this post.");

            var deleted = await postsService.DeletePostAsync(postId, userId);

            if (deleted)
            {
                var userPosts = await postsService.GetUserPostsAsync(userId);

                return Ok(new { StatusCode = StatusCodes.Status204NoContent, userPosts });
            }
            return BadRequest("Post could not be deleted.");
        }

        [HttpPatch("like-post/{authorId}/{postId}")]
        [HttpPatch("unlike-post/{authorId}/{postId}")]
        public async Task<IActionResult> LikePost(string authorId, string postId)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized("No valid token given with request.");

            var user = await userManager.GetUserByIdAsync(userId);

            if (HttpContext.Request.Path.Value!.Contains("unlike"))
            {
                if (user.LikedPosts.Find(p => p.Id == postId) is null)
                    return BadRequest("You have not liked this post.");

                var updatedPostUser = await postsService.LikePostAsync(postId, authorId, userId, true);

                return Ok(updatedPostUser);
            }
            else
            {
                if (user.LikedPosts.Find(p => p.Id == postId) is not null)
                    return BadRequest("You have already liked this post.");

                var updatedPostUser = await postsService.LikePostAsync(postId, authorId, userId);

                return Ok(updatedPostUser);
            }
        }
    }
}
