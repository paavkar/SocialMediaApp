namespace SocialMediaApp.Server.Models
{
    public class AuthorDTO : Author
    {
        public List<Author> Followers { get; set; }
        public List<Author> Following { get; set; }
    }
}
