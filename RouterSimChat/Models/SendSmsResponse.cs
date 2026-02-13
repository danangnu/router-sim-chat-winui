using System.Text.Json.Serialization;

namespace RouterSimChat.Models
{
    public sealed class SendSmsResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("outbox_id")]
        public long OutboxId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
    }

    public sealed class OutboxStatusResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("router_id")]
        public int RouterId { get; set; }

        [JsonPropertyName("slot")]
        public string Slot { get; set; } = "";

        [JsonPropertyName("peer")]
        public string Peer { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("attempts")]
        public int Attempts { get; set; }

        [JsonPropertyName("max_attempts")]
        public int MaxAttempts { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = "";

        [JsonPropertyName("sent_at")]
        public string? SentAt { get; set; }

        [JsonPropertyName("delivered_at")]
        public string? DeliveredAt { get; set; }

        [JsonPropertyName("provider_id")]
        public string? ProviderId { get; set; }

        [JsonPropertyName("last_error")]
        public string? LastError { get; set; }
    }
}