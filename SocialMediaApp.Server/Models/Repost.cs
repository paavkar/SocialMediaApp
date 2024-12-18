using Newtonsoft.Json;

namespace SocialMediaApp.Server.Models
{
    public class Repost : Post
    {
        [JsonProperty(PropertyName = "repostedBy")]
        public Author RepostedBy { get; set; }

        [JsonProperty(PropertyName = "repostedAt")]
        public DateTimeOffset RepostedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
