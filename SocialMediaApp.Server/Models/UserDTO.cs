namespace SocialMediaApp.Server.Models
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<Post> LikedPosts { get; set; } = new();
        public List<Repost> RepostedPosts { get; set; } = new();
        public List<Post> Bookmarks { get; set; } = new();
        public bool IsPrivate { get; set; } = false;
        public AccountSettings AccountSettings { get; set; }
    }
}
