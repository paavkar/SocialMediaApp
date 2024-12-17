using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SocialMediaApp.Server.CosmosDb;
using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.Services
{
    public class UserManager(ICosmosDbUserService cosmosDbUserService, RoleManager roleManager)
    {
        public async Task<UserAccount> CreateUserAsync(RegisterDTO registerDTO)
        {
            var roleExists = await roleManager.CreateRoleIfNotExistAsync("User");

            if (!roleExists) return null;

            string passwordHash = HashPassword(registerDTO.Password);

            var account = new UserAccount() { DisplayName = registerDTO.DisplayName, UserName = registerDTO.UserName, Email = registerDTO.Email, PasswordHash = passwordHash };
            var user = await cosmosDbUserService.AddAsync(account);

            return user;
        }

        public string HashPassword(string password)
        {
            byte[] salt = BitConverter.GetBytes(128 / 8);
            string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password!,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

            return passwordHash;
        }

        public async Task<UserAccount> GetUserByEmailAsync(string email)
        {
            var user = await cosmosDbUserService.GetByEmailAsync(email);

            return user;
        }
    }
}
