using Newtonsoft.Json;
using SocialMediaApp.Server.Models.DTOs;

namespace SocialMediaApp.Server.Models
{
    public class UserAccount
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [JsonProperty(PropertyName = "profilePicture")]
        public string ProfilePicture { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "passwordHash")]
        public string PasswordHash { get; set; }

        [JsonProperty(PropertyName = "likedPosts")]
        public List<Post> LikedPosts { get; set; } = [];

        [JsonProperty(PropertyName = "repostedPosts")]
        public List<Repost> RepostedPosts { get; set; } = [];

        [JsonProperty(PropertyName = "bookmarks")]
        public List<Post> Bookmarks { get; set; } = [];

        [JsonProperty(PropertyName = "accountSettings")]
        public AccountSettings AccountSettings { get; set; } = new();

        [JsonProperty(PropertyName = "following")]
        public List<Author> Following { get; set; } = [];

        [JsonProperty(PropertyName = "followers")]
        public List<Author> Followers { get; set; } = [];

        [JsonProperty(PropertyName = "followRequests")]
        public List<Author> FollowRequests { get; set; } = [];

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = "User";

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "UserAccount";

        [JsonProperty(PropertyName = "changeFeed")]
        public bool ChangeFeed { get; set; } = false;

        public UserDTO ToUserDTO()
        {
            return new UserDTO
            {
                Id = this.Id,
                ProfilePicture = this.ProfilePicture,
                DisplayName = this.DisplayName,
                Description = this.Description,
                UserName = this.UserName,
                Email = this.Email,
                RepostedPosts = this.RepostedPosts,
                AccountSettings = this.AccountSettings,
                Following = this.Following,
                Followers = this.Followers,
                FollowRequests = this.FollowRequests
            };
        }

        public AuthorDTO ToAuthorDTO()
        {
            return new AuthorDTO
            {
                Id = this.Id,
                ProfilePicture = this.ProfilePicture,
                DisplayName = this.DisplayName,
                Description = this.Description,
                UserName = this.UserName,
                Followers = this.Followers,
                Following = this.Following
            };
        }
    }
}
