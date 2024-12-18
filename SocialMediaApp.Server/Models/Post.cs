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
        public List<Post> Quotes { get; set; } = [];

        [JsonProperty(PropertyName = "replies")]
        public List<Post> Replies { get; set; } = [];

        [JsonProperty(PropertyName = "createdAt")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [JsonProperty(PropertyName = "author")]
        public Author Author { get; set; }

        [JsonProperty(PropertyName = "labels")]
        public List<string> Labels { get; set; } = [];

        [JsonProperty(PropertyName = "langs")]
        public List<string> Langs { get; set; } = [];

        [JsonProperty(PropertyName = "quotedPost")]
        public Post? QuotedPost { get; set; }

        [JsonProperty(PropertyName = "parentPost")]
        public Post? ParentPost { get; set; }

        [JsonProperty(PropertyName = "previousVersions")]
        public List<Post>? PreviousVersions { get; set; }

        [JsonProperty(PropertyName = "bookmarkCount")]
        public int BookmarkCount { get; set; }

        [JsonProperty(PropertyName = "isPinned")]
        public bool IsPinned { get; set; } = false;

        [JsonProperty(PropertyName = "partitionKey")]
        public string? PartitionKey { get; set; }
    }
}
