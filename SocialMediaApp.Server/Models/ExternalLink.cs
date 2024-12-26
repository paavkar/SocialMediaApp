using Newtonsoft.Json;

namespace SocialMediaApp.Server.Models
{
    public class ExternalLink
    {
        [JsonProperty(PropertyName = "externalLinkUri")]
        public string? ExternalLinkUri { get; set; }

        [JsonProperty(PropertyName = "externalLinkTitle")]
        public string? ExternalLinkTitle { get; set; }

        [JsonProperty(PropertyName = "externalLinkDescription")]
        public string? ExternalLinkDescription { get; set; }
    }
}
