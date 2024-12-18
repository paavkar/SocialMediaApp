using Newtonsoft.Json;

namespace SocialMediaApp.Server.Models
{
    public class UserAccount
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "passwordHash")]
        public string PasswordHash { get; set; }

        [JsonProperty(PropertyName = "likedPosts")]
        public List<Post> LikedPosts { get; set; } = new();

        [JsonProperty(PropertyName = "repostedPosts")]
        public List<Repost> RepostedPosts { get; set; } = new();

        [JsonProperty(PropertyName = "bookmarks")]
        public List<Post> Bookmarks { get; set; } = new();

        [JsonProperty(PropertyName = "accountSettings")]
        public AccountSettings AccountSettings { get; set; } = new();

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = "User";

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "UserAccount";
    }
}
