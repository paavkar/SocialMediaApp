using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Services;
using System.Security.Claims;

namespace SocialMediaApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(UserManager userManager) : ControllerBase
    {
        [HttpGet("{userName}")]
        public async Task<IActionResult> GetUser(string userName)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            var user = await userManager.GetUserByUserNameAsync(userName);

            if (user is null)
                return NotFound(new { Message = "No user found with given username." });

            if (user.AccountSettings.SignInRequired && String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "This user requires sign-in to view their profile." });
            if (userId != user.Id && user.AccountSettings.IsPrivate && !user.Followers.Any(f => f.Id == userId))
                return Unauthorized(new
                {
                    Message = "This user's profile is private.",
                    User = new Author()
                    {
                        Id = user.Id,
                        DisplayName = user.DisplayName,
                        Description = user.Description,
                        UserName = user.UserName
                    }
                });

            var userDto = user.ToUserDTO();

            return Ok(userDto);
        }

        [HttpPatch("follow-user/{userName}")]
        public async Task<IActionResult> Follow(string userName, [FromBody] Author follower)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized(new { Messge = "No valid token given with request." });
            if (follower is null)
                return BadRequest(new { Message = "No follower provided." });
            if (follower.Id != userId)
                return BadRequest(new { Message = "Follower ID does not match token ID." });
            if (follower.UserName == userName)
                return BadRequest(new { Message = "You are not allowed to follow yourself." });

            var updatedUser = await userManager.FollowAsync(userName, follower);

            if (updatedUser is null)
                return NotFound(new
                {
                    Message = $"Couldn't find a user with the username {userName}" +
                    $" or with the id {userId}"
                });

            var userDto = updatedUser.ToUserDTO();

            return Ok(new { Message = "", userDto });

        }

        [HttpPatch("confirm-follow/{userName}")]
        public async Task<IActionResult> ConfirmFollow(string userName, [FromBody] Author follower)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "No valid token given with request." });
            if (follower is null)
                return BadRequest(new { Message = "No follower provided." });

            var user = await userManager.GetUserByUserNameAsync(userName);

            if (!user.FollowRequests.Any(u => u.UserName == follower.UserName))
                return BadRequest(new { Message = "No follow request found from this user." });

            var updatedUser = await userManager.ConfirmFollowAsync(userName, follower);

            if (updatedUser is null)
                return NotFound(new
                {
                    Message = $"Couldn't find a user with the username {userName}" +
                    $" or with the id {userId}"
                });

            var userDto = updatedUser.ToUserDTO();

            return Ok(new { Message = "Accepted the follow request.", userDto });
        }

        [HttpGet("search-users/{searchTerm}")]
        public async Task<IActionResult> SearchUsers(string searchTerm)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "No valid token given with request." });

            var users = await userManager.MatchingUsersAsync(searchTerm);

            if (users is null || users.Count == 0)
                return NotFound(new { Message = "No users found with given search term." });

            var userDtos = users.Select(u => u.ToUserDTO()).ToList();

            return Ok(userDtos);
        }
    }
}
