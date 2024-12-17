using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.Services
{
    public class SignInManager(UserManager userManager, RoleManager roleManager)
    {
        public async Task<UserAccount> PasswordSignInAsync(LoginDTO loginDTO)
        {
            var user = await userManager.GetUserByEmailAsync(loginDTO.Email);
            var hashedPassword = userManager.HashPassword(loginDTO.Password);

            if (user is not null && hashedPassword == user.PasswordHash)
            {
                var userRole = await roleManager.GetUserRoleAsync(user);

                return user;
            }
            else return null;
        }

        public async Task<bool> AttemptPasswordSignInAsync(LoginDTO loginDTO)
        {
            var user = await userManager.GetUserByEmailAsync(loginDTO.Email);
            var hashedPassword = userManager.HashPassword(loginDTO.Password);

            return user is not null && hashedPassword == user.PasswordHash;
        }
    }
}
