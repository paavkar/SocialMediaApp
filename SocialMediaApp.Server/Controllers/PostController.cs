using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Services;
using System.Security.Claims;

namespace SocialMediaApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly UserManager _userManager;
        private readonly PostsService _postsService;

        public PostController(UserManager userManager, PostsService postsService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _postsService = postsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            if (String.IsNullOrEmpty(userId)) return Unauthorized("No valid token given with request.");

            var posts = await _postsService.GetAllPostsAsync();

            return Ok(posts);
        }

        [HttpGet("user-posts")]
        public async Task<IActionResult> GetUserPosts()
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId)) return Unauthorized("No valid token given with request.");

            var user = await _userManager.GetUserByIdAsync(userId);

            var posts = await _postsService.GetUserPostsAsync(userId);

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
            }

            foreach (var post in postDictionary)
            {
                chronologicalPosts.Add(post.Value);
            }

            return Ok(chronologicalPosts);
        }

        [HttpPost("post")]
        public async Task<IActionResult> Post([FromBody] Post post)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            if (String.IsNullOrEmpty(userId)) return Unauthorized("No valid token given with request.");

            var user = await _userManager.GetUserByIdAsync(userId);

            post.PartitionKey = $"Post-{userId}";

            var createdPost = await _postsService.CreatePostAsync(post);

            return CreatedAtAction(nameof(Post), createdPost);
        }

        [HttpDelete("delete-post/{postId}")]
        public async Task<IActionResult> DeletePost(string postId, [FromBody] Author postAuthor)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            if (String.IsNullOrEmpty(userId)) return Unauthorized("No valid token given with request.");

            if (userId != postAuthor.Id) return Unauthorized("You are not authorized to delete this post.");

            var deleted = await _postsService.DeletePostAsync(postId, userId);

            if (deleted) return Ok("Post deleted successfully.");
            return BadRequest("Post could not be deleted.");
        }

        [HttpPatch("like-post/{postId}")]
        [HttpPatch("unlike-post/{postId}")]
        public async Task<IActionResult> LikePost(string postId)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            Console.WriteLine(HttpContext.Request.Path.Value);

            if (String.IsNullOrEmpty(userId)) return Unauthorized("No valid token given with request.");

            if (HttpContext.Request.Path.Value!.Contains("unlike"))
            {
                var updatedPostUser = await _postsService.LikePostAsync(postId, userId, true);

                return Ok(updatedPostUser);
            }
            else
            {
                var updatedPostUser = await _postsService.LikePostAsync(postId, userId);

                return Ok(updatedPostUser);
            }
        }
    }
}
