using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Services;

namespace SocialMediaApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(UserManager userManager, SignInManager signInManager) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            var user = await userManager.CreateUserAsync(registerDto);

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

            if (user is not null) return CreatedAtAction(nameof(Register), userDto);
            else return BadRequest(new { Message = "There was an issue creating the account. Please try again later", StatusCode = StatusCodes.Status400BadRequest });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var userToken = await signInManager.PasswordSignInAsync(loginDto);

            if (userToken is not null) return Ok(userToken);

            return Unauthorized(new { Message = "There was an issue logging in. Check your email and password.", StatusCode = StatusCodes.Status401Unauthorized });
        }
    }
}
