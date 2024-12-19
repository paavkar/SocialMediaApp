using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Services;
using System.Security.Claims;

namespace SocialMediaApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager _userManager;

        public UserController(UserManager userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet("{userName}")]
        public async Task<IActionResult> GetUser(string userName)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            var user = await _userManager.GetUserByUserNameAsync(userName);

            if (user is null) return NotFound("No user found with given username.");

            if (user.AccountSettings.SignInRequired && String.IsNullOrEmpty(userId))
                return Unauthorized("This user requires sign-in to view their profile.");

            var userDto = new UserDTO()
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                UserName = user.UserName,
                Email = user.Email,
                LikedPosts = user.LikedPosts,
                RepostedPosts = user.RepostedPosts,
                Bookmarks = user.Bookmarks,
                AccountSettings = user.AccountSettings,
                Followers = user.Followers,
                Following = user.Following
            };

            return Ok(userDto);
        }

        [HttpPatch("follow-user/{userName}")]
        [HttpPatch("unfollow-user/{userName}")]
        public async Task<IActionResult> Follow(string username, [FromBody] Author follower)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            if (String.IsNullOrEmpty(userId)) return Unauthorized("No valid token given with request.");

            if (follower is null) return BadRequest("No follower provided.");

            if (follower.Id != userId) return BadRequest("Follower ID does not match token ID.");

            if (follower.UserName == username) return BadRequest("You are not allowed to follow yourself.");

            if (HttpContext.Request.Path.Value!.Contains("follow-user"))
            {
                var user = await _userManager.FollowAsync(username, follower);

                return Ok("User followed successfully.");
            }
            else
            {
                var user = await _userManager.FollowAsync(username, follower, false);

                return Ok("User unfollowed successfully.");
            }

            return Ok();
        }
    }
}
