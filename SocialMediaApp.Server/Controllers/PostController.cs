﻿using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Models.DTOs;
using SocialMediaApp.Server.Services;
using System.Security.Claims;

namespace SocialMediaApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController(UserManager userManager, PostsService postsService, ImageService imageService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "No valid token given with request." });

            var posts = await postsService.GetAllPostsAsync();

            return Ok(posts);
        }

        [HttpGet("user-posts/{userName}")]
        public async Task<IActionResult> GetUserPosts(string userName)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            var user = await userManager.GetUserByUserNameAsync(userName);

            if (user.AccountSettings.SignInRequired && String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "This user requires sign-in to view their profile." });
            if (user.Id != userId && user.AccountSettings.IsPrivate && !user.Followers.Any(f => f.Id == userId))
                return Unauthorized(new { Message = "This user's profile is private." });

            var posts = await postsService.GetUserPostsAsync(user.Id);

            List<dynamic> chronologicalPosts = [];
            Dictionary<string, dynamic> postDictionary = [];

            foreach (var post in posts)
            {
                foreach (var repost in user.RepostedPosts)
                {
                    if (repost.RepostedAt > post.CreatedAt)
                    {
                        postDictionary.Remove(repost.Id);
                        postDictionary.Add(repost.Id, repost);
                    }
                    if (!postDictionary.ContainsKey(post.Id))
                        postDictionary.Add(post.Id, post);
                }
                if (!postDictionary.ContainsKey(post.Id))
                    postDictionary.Add(post.Id, post);
            }

            foreach (var post in postDictionary)
            {
                chronologicalPosts.Add(post.Value);
            }

            return Ok(chronologicalPosts);
        }

        [HttpGet("post/{userName}/{postId}")]
        public async Task<IActionResult> GetPost(string userName, string postId)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            var user = await userManager.GetUserByUserNameAsync(userName);

            if (user.AccountSettings.SignInRequired && String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "This user requires sign-in to view their profile." });
            if (user.Id != userId && user.AccountSettings.IsPrivate && !user.Followers.Any(f => f.Id == userId))
                return Unauthorized(new { Message = "This user's profile is private." });

            var post = await postsService.GetPostByIdAsync(postId, user.Id);

            if (post is null)
                return NotFound(new { Message = "Post not found." });

            return Ok(post);
        }

        [HttpPost("post")]
        public async Task<IActionResult> Post([FromBody] Post post)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "No valid token given with request." });

            var user = await userManager.GetUserByIdAsync(userId);

            post.PartitionKey = $"Post-{userId}";

            var createdPost = await postsService.CreatePostAsync(post);
            var userPosts = await postsService.GetUserPostsAsync(userId);

            return CreatedAtAction(nameof(Post), new { createdPost, userPosts });
        }

        [HttpDelete("delete-post/{postId}")]
        public async Task<IActionResult> DeletePost(string postId, [FromBody] Author postAuthor)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "No valid token given with request." });
            if (userId != postAuthor.Id)
                return Unauthorized(new { Message = "You are not authorized to delete this post." });

            var deleted = await postsService.DeletePostAsync(postId, userId);

            if (deleted)
            {
                var userPosts = await postsService.GetUserPostsAsync(userId);

                return Ok(new { StatusCode = StatusCodes.Status204NoContent, userPosts });
            }
            return BadRequest(new { Message = "Post could not be deleted." });
        }

        [HttpPatch("like-post/{authorId}/{postId}")]
        public async Task<IActionResult> LikePost(string authorId, string postId)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "No valid token given with request." });

            var user = await userManager.GetUserByIdAsync(userId);

            var updatedPostUser = await postsService.LikePostAsync(postId, authorId, userId);

            return Ok(updatedPostUser);
        }

        [HttpGet("search-posts/{searchTerm}")]
        public async Task<IActionResult> SearchPosts(string searchTerm)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "No valid token given with request." });

            var posts = await postsService.GetMatchingPostsAsync(searchTerm);

            if (posts is null || posts.Count == 0)
                return NotFound(new { Message = "No posts found with given search term." });

            return Ok(posts);
        }

        [HttpPatch("bookmark-post/{postId}")]
        public async Task<IActionResult> BookmarkPost(string postId, [FromBody] string postAuthorId)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;
            if (String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "No valid token given with request." });

            var updatedUser = await postsService.BookmarkPost(postId, postAuthorId, userId);

            return Ok(updatedUser);
        }

        [HttpPatch("update-post/{postId}")]
        public async Task<IActionResult> UpdatePost(string postId, [FromBody] PostUpdateDTO postUpdate)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            if (String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "No valid token given with request." });

            if (userId != postUpdate.AuthorId)
                return Unauthorized(new { Message = "You are not authorized to update this post." });

            var updatedPost = await postsService.UpdatePostTextAsync(postId, postUpdate.AuthorId, postUpdate.PostText);

            return Ok(updatedPost);
        }

        [HttpPost("upload-post-images/{postId}")]
        public async Task<IActionResult> UploadPostImages(string postId,
            [FromForm] List<IFormFile> images,
            [FromForm] List<string> altTexts,
            [FromForm] List<int> heights,
            [FromForm] List<int> widths)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.Sid)!;

            if (String.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "No valid token given with request." });

            if (images == null || images.Count == 0)
            {
                return BadRequest("Invalid image.");
            }

            if (images.Count > 4)
            {
                return BadRequest("You can upload a maximum of 4 images.");
            }

            var post = await postsService.GetPostByIdAsync(postId, userId);
            post.Embed.Images = [];

            for (int i = 0; i < images.Count; i++)
            {
                post.Embed.Images.Add(new Media());
                string fileExtension = Path.GetExtension(images[i].FileName);

                var fileName = $"image-{i + 1}-{postId}{fileExtension.ToLower()}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var filePath = Path.Combine(uploadsFolder, images[i].FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await images[i].CopyToAsync(stream);
                }

                string blobUri = await imageService.UploadPostImageAsync(images[i], fileName, postId, userId);

                System.IO.File.Delete(filePath);

                post.Embed.Images[i].AltText = altTexts[i];
                AspectRatio aspectRatio = new() { Height = heights[i], Width = widths[i] };
                post.Embed.Images[i].AspectRatio = aspectRatio;
                post.Embed.Images[i].FilePath = blobUri;
            }

            Post updatedPost = await imageService.UpdatePostMediaAsync(post);

            post = await postsService.GetPostDTOAsync(updatedPost);

            return Ok(new { createdPost = post });
        }
    }
}
