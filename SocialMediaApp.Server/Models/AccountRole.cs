using Newtonsoft.Json;

namespace SocialMediaApp.Server.Models
{
    public class AccountRole
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [JsonProperty(PropertyName = "roleName")]
        public string RoleName { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = "AccountRole";

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "AccountRole";
    }
}
