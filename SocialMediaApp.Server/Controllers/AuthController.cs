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

        public AuthController(UserManager userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserAccount account)
        {
            var user = await _userManager.CreateUserAsync(account);

            if (user is UserAccount) return CreatedAtAction(nameof(Register), user);
            else return BadRequest(new { Message = "There was an issue creating the account. Please try again later", StatusCode = StatusCodes.Status400BadRequest });
        }
    }
}
