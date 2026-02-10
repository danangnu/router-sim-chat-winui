using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RouterSimChat.Models;

namespace RouterSimChat.Services
{
    public sealed class ApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json;

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

        public async Task<List<MessageItem>> GetMessagesAsync(int routerId, string slot, string peer, int limit)
        {
            var url =
                $"api/messages?router_id={routerId}" +
                $"&slot={Uri.EscapeDataString(slot)}" +
                $"&peer={Uri.EscapeDataString(peer)}" +
                $"&limit={limit}";

            var json = await _http.GetStringAsync(url);
            System.Diagnostics.Trace.WriteLine($"[API] GET {url} -> {json}");

            return JsonSerializer.Deserialize<List<MessageItem>>(json, _json) ?? new List<MessageItem>();
        }
    }
}