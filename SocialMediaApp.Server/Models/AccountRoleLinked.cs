using Newtonsoft.Json;

namespace SocialMediaApp.Server.Models
{
    public class AccountRoleLinked
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [JsonProperty(PropertyName = "roleId")]
        public string RoleId { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }
    }
}
