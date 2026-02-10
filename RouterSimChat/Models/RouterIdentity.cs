using System.Text.Json.Serialization;

namespace RouterSimChat.Models
{
    public sealed class RouterIdentity
    {
        [JsonPropertyName("router_id")]
        public int RouterId { get; set; }

        [JsonPropertyName("router_name")]
        public string RouterName { get; set; } = "";

        [JsonPropertyName("hostname")]
        public string Hostname { get; set; } = "";

        [JsonPropertyName("ip")]
        public string Ip { get; set; } = "";

        // API uses "interface"
        [JsonPropertyName("interface")]
        public string Interface { get; set; } = "";

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }
    }
}