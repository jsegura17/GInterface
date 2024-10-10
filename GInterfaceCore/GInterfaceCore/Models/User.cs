using System.Text.Json.Serialization;

namespace GInterfaceCore.Models
{
    public class User
    {
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("Username")]
        public string Username { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }
        
        [JsonPropertyName("FullName")]
        public string FullName { get; set; }

        [JsonPropertyName("UserType")]
        public string UserType { get; set; }

        [JsonPropertyName("Site")]
        public string Site { get; set; }
    }
}
