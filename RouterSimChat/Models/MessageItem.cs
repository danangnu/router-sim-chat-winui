using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace RouterSimChat.Models
{
    public sealed class MessageItem : INotifyPropertyChanged
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";   // "outbox:5", "log:3", or local temp id

        [JsonPropertyName("router_id")]
        public int RouterId { get; set; }

        [JsonPropertyName("slot")]
        public string Slot { get; set; } = "";

        [JsonPropertyName("peer")]
        public string Peer { get; set; } = "";

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = ""; // "in" or "out"

        [JsonPropertyName("body")]
        public string Body { get; set; } = "";

        private string _ts = "";
        [JsonPropertyName("ts")]
        public string Ts
        {
            get => _ts;
            set { _ts = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimeShort)); }
        }

        [JsonPropertyName("outbox_id")]
        public long? OutboxId { get; set; }

        private string? _status;
        [JsonPropertyName("status")]
        public string? Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusLabel));
                OnPropertyChanged(nameof(IsFailed));
            }
        }

        [JsonPropertyName("attempts")]
        public int? Attempts { get; set; }

        [JsonPropertyName("max_attempts")]
        public int? MaxAttempts { get; set; }

        [JsonPropertyName("provider_id")]
        public string? ProviderId { get; set; }

        private string? _lastError;
        [JsonPropertyName("last_error")]
        public string? LastError
        {
            get => _lastError;
            set { _lastError = value; OnPropertyChanged(); }
        }

        // ---------- convenience properties ----------

        public string TimeShort =>
            !string.IsNullOrWhiteSpace(Ts) && Ts.Length >= 16
                ? Ts.Substring(11, 5)
                : Ts;

        public bool IsOutbound =>
            string.Equals(Direction, "out", System.StringComparison.OrdinalIgnoreCase);

        public bool IsFailed =>
            string.Equals(Status, "failed", System.StringComparison.OrdinalIgnoreCase);

        // Only show text when failed. (If you want “Sending…” later, add pending here.)
        public string StatusLabel => IsFailed ? "Failed" : string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}