using Microsoft.IdentityModel.Tokens;
using SocialMediaApp.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SocialMediaApp.Server.Services
{
    public class SignInManager(UserManager userManager, RoleManager roleManager, IConfiguration configuration)
    {
        public async Task<object> PasswordSignInAsync(LoginDTO loginDTO)
        {
            var user = await userManager.GetUserByEmailAsync(loginDTO.Email);
            var hashedPassword = userManager.HashPassword(loginDTO.Password);

            if (user is not null && hashedPassword == user.PasswordHash)
            {
                var userRole = await roleManager.GetUserRoleAsync(user);
                var userDto = new UserDTO()
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    UserName = user.UserName,
                    Email = user.Email,
                    LikedPosts = user.LikedPosts,
                    RepostedPosts = user.RepostedPosts,
                    Bookmarks = user.Bookmarks,
                    AccountSettings = user.AccountSettings
                };

                var token = await CreateToken(user);

                return new { User = userDto, Token = token };
            }
            else return null;
        }

        public async Task<bool> AttemptPasswordSignInAsync(LoginDTO loginDTO)
        {
            var user = await userManager.GetUserByEmailAsync(loginDTO.Email);
            var hashedPassword = userManager.HashPassword(loginDTO.Password);

            return user is not null && hashedPassword == user.PasswordHash;
        }

        public async Task<string> CreateToken(UserAccount account)
        {
            var userRole = await roleManager.GetUserRoleAsync(account);

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, account.UserName),
                new Claim(ClaimTypes.GivenName, account.DisplayName),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, userRole),
                new Claim(ClaimTypes.Sid, account.Id)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                configuration.GetSection("AppSettings:Token").Value));

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
