﻿using Microsoft.AspNetCore.Mvc;
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
                return NotFound("No user found with given username.");

            if (user.AccountSettings.SignInRequired && String.IsNullOrEmpty(userId))
                return Unauthorized("This user requires sign-in to view their profile.");
            if (user.AccountSettings.IsPrivate && user.Followers.Any(f => f.Id == userId))
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
        [HttpPatch("unfollow-user/{userName}")]
        public async Task<IActionResult> Follow(string username, [FromBody] Author follower)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized("No valid token given with request.");
            if (follower is null)
                return BadRequest("No follower provided.");
            if (follower.Id != userId)
                return BadRequest("Follower ID does not match token ID.");
            if (follower.UserName == username)
                return BadRequest("You are not allowed to follow yourself.");

            var user = await userManager.GetUserByIdAsync(userId);

            if (HttpContext.Request.Path.Value!.Contains("unfollow-user"))
            {
                if (user.Following.Find(u => u.UserName == username) is null)
                    return BadRequest("You are not following this user.");

                var updatedUser = await userManager.FollowAsync(username, follower, false);

                if (updatedUser is null)
                    return NotFound($"Couldn't find a user with the username {username}" +
                        $" or with the id {userId}");

                var userDto = updatedUser.ToUserDTO();

                return Ok(new { Message = "User unfollowed successfully.", userDto });
            }
            else
            {
                if (user.Following.Find(u => u.UserName == username) is not null)
                    return BadRequest("You are already following this user.");

                var updatedUser = await userManager.FollowAsync(username, follower);

                if (updatedUser is null)
                    return NotFound($"Couldn't find a user with the username {username}" +
                        $" or with the id {userId}");

                var userDto = updatedUser.ToUserDTO();

                return Ok(new { Message = "User followed successfully", userDto });
            }
        }
    }
}
