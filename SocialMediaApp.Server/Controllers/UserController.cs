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
    }
}
