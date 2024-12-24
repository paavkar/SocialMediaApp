using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SocialMediaApp.Server.CosmosDb;
using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.Services
{
    public class UserManager(ICosmosDbService cosmosDbService, RoleManager roleManager)
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
            var user = await cosmosDbService.AddAsync(account);

            return user;
        }

        public async Task<UserAccount> GetUserByIdAsync(string userId)
        {
            var user = await cosmosDbService.GetUserByIdAsync(userId);

            return user;
        }

        public async Task<UserAccount> GetUserByEmailOrUserNameAsync(string emailOrUserName)
        {
            var user = await cosmosDbService.GetUserByEmailOrUserNameAsync(emailOrUserName);

            return user;
        }

        public async Task<UserAccount> GetUserByEmailAsync(string email)
        {
            var user = await cosmosDbService.GetByEmailAsync(email);

            return user;
        }

        public async Task<UserAccount> GetUserByUserNameAsync(string userName)
        {
            var user = await cosmosDbService.GetUserByUserNameAsync(userName);

            return user;
        }

        public async Task<object> FollowAsync(string userName, Author follower)
        {
            var user = await cosmosDbService.FollowUserAsync(userName, follower);

            return user;
        }

        public async Task<object> ConfirmFollowAsync(string userName, Author follower)
        {
            var user = await cosmosDbService.ConfirmFollowAsync(userName, follower);

            return user;
        }

        public async Task<List<UserAccount>> MatchingUsersAsync(string searchTerm)
        {
            var users = await cosmosDbService.MatchingUsersAsync(searchTerm);

            return users;
        }

        public async Task<UserAccount> UpdateUserAsync(Author user)
        {
            var updatedUser = await cosmosDbService.UpdateUserAsync(user);

            return updatedUser;
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
