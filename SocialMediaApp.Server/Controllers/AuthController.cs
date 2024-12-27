using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Server.Models.DTOs;
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

            if (user is null)
                return BadRequest(new { Message = "Either the email or username is already taken." });

            if (user is not null) return CreatedAtAction(nameof(Register), user.ToUserDTO());
            else return BadRequest(new { Message = "There was an issue creating the account. Please try again later" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var userToken = await signInManager.PasswordSignInAsync(loginDto);

            if (userToken is not null) return Ok(userToken);

            return Unauthorized(new { Message = "There was an issue logging in. Check your email and password." });
        }
    }
}
