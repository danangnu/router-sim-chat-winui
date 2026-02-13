using RouterSimChat.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RouterSimChat.Services
{
    public sealed class ApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public ApiClient(string baseUrl)
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/")
            };

            _json = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<RouterIdentity>> GetRoutersAsync()
        {
            var url = "api/routers";
            var json = await _http.GetStringAsync(url);

            // Proof: show raw JSON (you will see router_id values here)
            System.Diagnostics.Trace.WriteLine($"[API] GET {url} -> {json}");

            // Proof: parse router_id directly from JSON
            using (var doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                {
                    var first = doc.RootElement[0];
                    if (first.TryGetProperty("router_id", out var rid))
                        System.Diagnostics.Trace.WriteLine($"[API] first.router_id = {rid.GetInt32()}");
                }
            }

            var data = JsonSerializer.Deserialize<List<RouterIdentity>>(json, _json);
            return data ?? new List<RouterIdentity>();
        }

        public async Task<List<ConversationItem>> GetConversationsAsync(int routerId, string slot, string? q)
        {
            // encode params properly
            var url = $"api/conversations?router_id={routerId}&slot={Uri.EscapeDataString(slot)}";
            if (!string.IsNullOrWhiteSpace(q))
                url += $"&q={Uri.EscapeDataString(q)}";

            var json = await _http.GetStringAsync(url);
            System.Diagnostics.Trace.WriteLine($"[API] GET {url} -> {json}");

            return JsonSerializer.Deserialize<List<ConversationItem>>(json, _json) ?? new List<ConversationItem>();
        }

        public async Task<List<MessageItem>> GetMessagesAsync(
            int routerId,
            string slot,
            string peer,
            int limit = 50,
            string? sinceTs = null)
        {
            var url = new StringBuilder();
            url.Append($"/api/messages?router_id={routerId}");
            url.Append($"&slot={Uri.EscapeDataString(slot)}");
            url.Append($"&peer={Uri.EscapeDataString(peer)}");
            url.Append($"&limit={limit}");

            if (!string.IsNullOrEmpty(sinceTs))
            {
                url.Append($"&since_ts={Uri.EscapeDataString(sinceTs)}");
            }

            using var resp = await _http.GetAsync(url.ToString());
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<List<MessageItem>>(json, _jsonOptions)
                        ?? new List<MessageItem>();

            return items;
        }

        public async Task<SendSmsResponse> SendSmsAsync(int routerId, string slot, string peer, string body)
        {
            var payload = new
            {
                router_id = routerId,
                slot,
                peer,
                body
            };

            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync("/api/send-sms", content);
            resp.EnsureSuccessStatusCode();

            await using var stream = await resp.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<SendSmsResponse>(stream, _jsonOptions)
                         ?? new SendSmsResponse { Ok = false };
            return result;
        }

        public async Task<OutboxStatusResponse> GetOutboxStatusAsync(long outboxId)
        {
            var resp = await _http.GetAsync($"/api/outbox-status/{outboxId}");
            resp.EnsureSuccessStatusCode();

            await using var stream = await resp.Content.ReadAsStreamAsync();
            return (await JsonSerializer.DeserializeAsync<OutboxStatusResponse>(stream, _jsonOptions))!;
        }

    }
}