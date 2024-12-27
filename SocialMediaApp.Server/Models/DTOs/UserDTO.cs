namespace SocialMediaApp.Server.Models.DTOs
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string ProfilePicture { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<PostDTO> LikedPosts { get; set; } = [];
        public List<Repost> RepostedPosts { get; set; } = [];
        public List<PostDTO> Bookmarks { get; set; } = [];
        public AccountSettings AccountSettings { get; set; }
        public List<Author> Following { get; set; } = [];
        public List<Author> Followers { get; set; } = [];
        public List<Author> FollowRequests { get; set; } = [];
    }
}
