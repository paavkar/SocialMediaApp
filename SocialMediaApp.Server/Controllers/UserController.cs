using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Services;
using System.Security.Claims;

namespace SocialMediaApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(UserManager userManager, ImageService imageService) : ControllerBase
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
                        ProfilePicture = user.ProfilePicture,
                        DisplayName = user.DisplayName,
                        Description = user.Description,
                        UserName = user.UserName
                    }
                });

            var userDto = user.ToUserDTO();

            user.LikedPosts.Reverse();

            foreach (var likedPost in user.LikedPosts)
            {
                var author = await userManager.GetUserByIdAsync(likedPost.Author.Id);
                userDto.LikedPosts.Add(likedPost.ToPostDTO(author));
            }

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

            return Ok(new { Message = "", updatedUser });

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

            return Ok(new { Message = "Accepted the follow request.", updatedUser });
        }

        [HttpGet("user-followers/{userId}")]
        public async Task<IActionResult> GetFollowersAsync(string userId)
        {
            string tokenUserId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(tokenUserId))
                return Unauthorized(new { Message = "No valid token given with request." });

            var followers = await userManager.GetUserFollowersAsync(userId);

            if (followers is null || followers.Count == 0)
                return NotFound(new { Message = "No followers found for this user." });

            var followerDtos = followers.Select(f => f.ToUserDTO()).ToList();

            return Ok(followerDtos);
        }

        [HttpGet("user-followings/{userId}")]
        public async Task<IActionResult> GetUserFollowingsAsync(string userId)
        {
            string tokenUserId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(tokenUserId))
                return Unauthorized(new { Message = "No valid token given with request." });

            var followings = await userManager.GetUserFollowingsAsync(userId);

            if (followings is null || followings.Count == 0)
                return NotFound(new { Message = "No followings found for this user." });

            var followingDtos = followings.Select(f => f.ToUserDTO()).ToList();

            return Ok(followingDtos);
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

        [HttpPatch("update-user/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] Author user)
        {
            string tokenUserId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            if (String.IsNullOrEmpty(tokenUserId))
                return Unauthorized(new { Message = "No valid token given with request." });
            if (userId != tokenUserId)
                return Unauthorized(new { Message = "Token ID does not match user ID." });
            if (user is null)
                return BadRequest(new { Message = "No user provided." });

            var updatedUser = await userManager.UpdateUserAsync(user);

            if (updatedUser is null)
                return NotFound(new { Message = "No user found with given ID." });

            var userDto = updatedUser.ToUserDTO();

            updatedUser.LikedPosts.Reverse();

            foreach (var likedPost in updatedUser.LikedPosts)
            {
                var author = await userManager.GetUserByIdAsync(likedPost.Author.Id);
                userDto.LikedPosts.Add(likedPost.ToPostDTO(author));
            }

            return Ok(userDto);
        }

        [HttpPost("upload-profile-picture/{userId}")]
        public async Task<IActionResult> UploadProfilePicture(string userId, [FromForm] IFormFile image)
        {
            string tokenUserId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            if (String.IsNullOrEmpty(tokenUserId))
                return Unauthorized(new { Message = "No valid token given with request." });
            if (userId != tokenUserId)
                return Unauthorized(new { Message = "Token ID does not match user ID." });

            if (image == null || image.Length == 0)
            {
                return BadRequest("Invalid image.");
            }

            string fileExtension = Path.GetExtension(image.FileName);

            var fileName = $"profile-{userId}{fileExtension}";

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine("uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            string blobUri = await imageService.UploadImageAsync(image, fileName, userId);

            var user = await userManager.GetUserByIdAsync(userId);

            var userDto = user.ToUserDTO();

            user.LikedPosts.Reverse();

            foreach (var likedPost in user.LikedPosts)
            {
                var author = await userManager.GetUserByIdAsync(likedPost.Author.Id);
                userDto.LikedPosts.Add(likedPost.ToPostDTO(author));
            }

            System.IO.File.Delete(filePath);

            return Ok(new { Url = blobUri, user = userDto });
        }
    }
}
