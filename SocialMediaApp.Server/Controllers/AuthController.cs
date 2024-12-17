using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Services;

namespace SocialMediaApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager _userManager;
        private readonly SignInManager _signInManager;

        public AuthController(UserManager userManager, SignInManager signInManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            var user = await _userManager.CreateUserAsync(registerDto);

            if (user is not null) return CreatedAtAction(nameof(Register), user);
            else return BadRequest(new { Message = "There was an issue creating the account. Please try again later", StatusCode = StatusCodes.Status400BadRequest });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var user = await _signInManager.PasswordSignInAsync(loginDto);

            if (user is not null) return Ok(user);

            return Unauthorized(new { Message = "There was an issue logging in. Check your email and password.", StatusCode = StatusCodes.Status401Unauthorized });
        }
    }
}
