using System.Text.Json.Serialization;

namespace RouterSimChat.Models
{
    public sealed class MessageItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("router_id")]
        public int RouterId { get; set; }

        [JsonPropertyName("slot")]
        public string Slot { get; set; } = "";

        [JsonPropertyName("peer")]
        public string Peer { get; set; } = "";

        [JsonPropertyName("body")]
        public string Body { get; set; } = "";

        [JsonPropertyName("ts")]
        public string Ts { get; set; } = "";

        public string TimeShort => !string.IsNullOrWhiteSpace(Ts) && Ts.Length >= 16
            ? Ts.Substring(11, 5)
            : Ts;
    }
}