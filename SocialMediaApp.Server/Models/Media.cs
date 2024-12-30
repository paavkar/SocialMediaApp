using Newtonsoft.Json;

namespace SocialMediaApp.Server.Models
{
    public class Media
    {
        [JsonProperty(PropertyName = "altText")]
        public string? AltText { get; set; }

        [JsonProperty(PropertyName = "aspectRatio")]
        public AspectRatio? AspectRatio { get; set; }

        [JsonProperty(PropertyName = "filePath")]
        public string? FilePath { get; set; }
    }
}
