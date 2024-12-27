namespace SocialMediaApp.Server.Models.DTOs
{
    public class PostDTO
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int LikeCount { get; set; }
        public int RepostCount { get; set; }
        public int QuoteCount { get; set; }
        public int ReplyCount { get; set; }
        public List<string> ReplyIds { get; set; }
        public List<Author> AccountsLiked { get; set; }
        public List<Author> AccountsReposted { get; set; }
        public List<PostDTO> Quotes { get; set; }
        public List<PostDTO> Replies { get; set; } = [];
        public List<PostDTO> Thread { get; set; }
        public List<PostDTO> AuthorThread { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public AuthorDTO Author { get; set; }
        public List<string> Labels { get; set; }
        public List<string> Langs { get; set; }
        public Post? QuotedPost { get; set; }
        public Post? ParentPost { get; set; }
        public List<Post>? PreviousVersions { get; set; }
        public int BookmarkCount { get; set; }
        public bool IsPinned { get; set; } = false;
        public string? PartitionKey { get; set; }
    }
}
