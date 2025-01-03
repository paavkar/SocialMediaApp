﻿using Microsoft.IdentityModel.Tokens;
using SocialMediaApp.Server.Models;
using SocialMediaApp.Server.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SocialMediaApp.Server.Services
{
    public class SignInManager(UserManager userManager, RoleManager roleManager, IConfiguration configuration)
    {
        public async Task<object> PasswordSignInAsync(LoginDTO loginDTO)
        {
            var user = await userManager.GetUserByEmailOrUserNameAsync(loginDTO.EmailOrUserName);
            var hashedPassword = userManager.HashPassword(loginDTO.Password);

            if (user is not null && hashedPassword == user.PasswordHash)
            {
                var userRole = await roleManager.GetUserRoleAsync(user);

                var token = await CreateToken(user, userRole);

                var userDto = user.ToUserDTO();

                foreach (var likedPost in user.LikedPosts)
                {
                    var author = await userManager.GetUserByIdAsync(likedPost.Author.Id);
                    userDto.LikedPosts.Add(likedPost.ToPostDTO(author));
                }

                return new { User = userDto, Token = token };
            }
            else return null;
        }

        public async Task<bool> AttemptPasswordSignInAsync(LoginDTO loginDTO)
        {
            var user = await userManager.GetUserByEmailOrUserNameAsync(loginDTO.EmailOrUserName);
            var hashedPassword = userManager.HashPassword(loginDTO.Password);

            return user is not null && hashedPassword == user.PasswordHash;
        }

        public async Task<string> CreateToken(UserAccount account, string userRole)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, account.UserName),
                new Claim(ClaimTypes.GivenName, account.DisplayName),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, userRole),
                new Claim(ClaimTypes.Sid, account.Id)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
