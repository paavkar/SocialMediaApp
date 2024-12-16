namespace SocialMediaApp.Server.CosmosDb
{
    public class CosmosDbSettings
    {
        public static string Section = "CosmosDb";
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
    }
}
