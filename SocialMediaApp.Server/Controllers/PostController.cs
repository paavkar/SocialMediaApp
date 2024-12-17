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

            var posts = await _postsService.GetUserPostsAsync(userId);

            return Ok(posts);
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
    }
}
