using Newtonsoft.Json;

namespace SocialMediaApp.Server.Models
{
    public class AspectRatio
    {
        [JsonProperty(PropertyName = "height")]
        public int? Height { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int? Width { get; set; }
    }
}
