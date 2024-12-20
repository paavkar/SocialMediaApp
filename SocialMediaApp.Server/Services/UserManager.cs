using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SocialMediaApp.Server.CosmosDb;
using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.Services
{
    public class UserManager(ICosmosDbService cosmosDbUserService, RoleManager roleManager)
    {
        public async Task<UserAccount> CreateUserAsync(RegisterDTO registerDTO)
        {
            var roleExists = await roleManager.CreateRoleIfNotExistAsync("User");

            if (!roleExists) return null;

            string passwordHash = HashPassword(registerDTO.Password);

            var account = new UserAccount()
            {
                DisplayName = registerDTO.DisplayName,
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
                PasswordHash = passwordHash
            };
            var user = await cosmosDbUserService.AddAsync(account);

            return user;
        }

        public async Task<UserAccount> GetUserByIdAsync(string userId)
        {
            var user = await cosmosDbUserService.GetUserAsync(userId);

            return user;
        }

        public async Task<UserAccount> GetUserByEmailOrUserNameAsync(string emailOrUserName)
        {
            var user = await cosmosDbUserService.GetUserByEmailOrUserNameAsync(emailOrUserName);

            return user;
        }

        public async Task<UserAccount> GetUserByEmailAsync(string email)
        {
            var user = await cosmosDbUserService.GetByEmailAsync(email);

            return user;
        }

        public async Task<UserAccount> GetUserByUserNameAsync(string userName)
        {
            var user = await cosmosDbUserService.GetUserByUserNameAsync(userName);

            return user;
        }

        public async Task<UserAccount> FollowAsync(string userName, Author follower, bool follow = true)
        {
            var user = await cosmosDbUserService.FollowUserAsync(userName, follower, follow);

            return user;
        }

        public string HashPassword(string password)
        {
            byte[] salt = BitConverter.GetBytes(16);
            string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password!,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

            return passwordHash;
        }
    }
}
