using System.Text.Json.Serialization;

namespace RouterSimChat.Models
{
    public sealed class ConversationItem
    {
        [JsonPropertyName("router_id")]
        public int RouterId { get; set; }

        [JsonPropertyName("slot")]
        public string Slot { get; set; } = "";

        [JsonPropertyName("peer")]
        public string Peer { get; set; } = "";

        [JsonPropertyName("last_message")]
        public string LastMessage { get; set; } = "";

        [JsonPropertyName("last_at")]
        public string LastAt { get; set; } = "";

        [JsonPropertyName("message_count")]
        public int MessageCount { get; set; }

        public string TimeShort => !string.IsNullOrWhiteSpace(LastAt) && LastAt.Length >= 16
            ? LastAt.Substring(11, 5)
            : LastAt;
    }
}