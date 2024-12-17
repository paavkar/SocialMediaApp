using Newtonsoft.Json;

namespace SocialMediaApp.Server.Models
{
    public class Post
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "likeCount")]
        public int LikeCount { get; set; }

        [JsonProperty(PropertyName = "repostCount")]
        public int RepostCount { get; set; }

        [JsonProperty(PropertyName = "quoteCount")]
        public int QuoteCount { get; set; }

        [JsonProperty(PropertyName = "replyCount")]
        public int ReplyCount { get; set; }

        [JsonProperty(PropertyName = "quotes")]
        public List<Post> Quotes { get; set; } = new();

        [JsonProperty(PropertyName = "replies")]
        public List<Post> Replies { get; set; } = new();

        [JsonProperty(PropertyName = "createdAt")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [JsonProperty(PropertyName = "author")]
        public UserDTO Author { get; set; }

        [JsonProperty(PropertyName = "labels")]
        public List<string> Labels { get; set; } = new();

        [JsonProperty(PropertyName = "langs")]
        public List<string> Langs { get; set; } = new();

        [JsonProperty(PropertyName = "quotedPost")]
        public Post? QuotedPost { get; set; }

        [JsonProperty(PropertyName = "previousVersions")]
        public List<Post>? PreviousVersions { get; set; }

        [JsonProperty(PropertyName = "repostedBy")]
        public UserDTO? RepostedBy { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string? PartitionKey { get; set; }
    }
}
