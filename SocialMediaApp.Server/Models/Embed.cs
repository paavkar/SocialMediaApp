using Newtonsoft.Json;
using static SocialMediaApp.Server.Models.Enums;

namespace SocialMediaApp.Server.Models
{
    public class Embed
    {
        [JsonProperty(PropertyName = "embedType")]
        public EmbedType EmbedType { get; set; } = EmbedType.None;

        [JsonProperty(PropertyName = "images")]
        public List<Media>? Images { get; set; }

        [JsonProperty(PropertyName = "videos")]
        public List<Media>? Videos { get; set; }

        [JsonProperty(PropertyName = "externalLink")]
        public ExternalLink? ExternalLink { get; set; }
    }
}
