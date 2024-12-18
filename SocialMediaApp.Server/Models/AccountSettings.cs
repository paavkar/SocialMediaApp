using Newtonsoft.Json;

namespace SocialMediaApp.Server.Models
{
    public class AccountSettings
    {
        [JsonProperty(PropertyName = "signInRequired")]
        public bool SignInRequired { get; set; } = false;

        [JsonProperty(PropertyName = "isPrivate")]
        public bool IsPrivate { get; set; } = false;
    }
}
