namespace SocialMediaApp.Server.Models
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<Post> LikedPosts { get; set; } = [];
        public List<Repost> RepostedPosts { get; set; } = [];
        public List<Post> Bookmarks { get; set; } = [];
        public AccountSettings AccountSettings { get; set; }
        public List<UserDTO> Following { get; set; } = [];
        public List<UserDTO> Followers { get; set; } = [];
    }
}
