namespace SocialMediaApp.Server.Models.DTOs
{
    public class AuthorDTO : Author
    {
        public List<Author> Followers { get; set; }
        public List<Author> Following { get; set; }
    }
}
