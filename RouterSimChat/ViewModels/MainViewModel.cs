using RouterSimChat.Models;
using RouterSimChat.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;

namespace RouterSimChat.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _api;
        private CancellationTokenSource? _messagesPollCts;
        private string? _lastMessageTs;   // newest ts we’ve seen for current conversation

        public ObservableCollection<RouterIdentity> Routers { get; } = new();
        public ObservableCollection<ConversationItem> Conversations { get; } = new();
        public ObservableCollection<MessageItem> Messages { get; } = new();

        private RouterIdentity? _selectedRouter;
        public RouterIdentity? SelectedRouter
        {
            get => _selectedRouter;
            set
            {
                if (_selectedRouter == value) return;
                _selectedRouter = value;
                OnPropertyChanged();

                _messagesPollCts?.Cancel();
                _messagesPollCts = null;
                _lastMessageTs = null;

                _ = LoadConversationsAsync();
            }
        }

        private ConversationItem? _selectedConversation;
        public ConversationItem? SelectedConversation
        {
            get => _selectedConversation;
            set
            {
                if (_selectedConversation == value) return;
                _selectedConversation = value;
                OnPropertyChanged();

                // full refresh once when selecting conversation
                _ = LoadMessagesAsync();

                // restart incremental poll
                StartMessagesPolling();
            }
        }

        private string _routerSearch = "";
        public string RouterSearch
        {
            get => _routerSearch;
            set { _routerSearch = value; OnPropertyChanged(); }
        }

        private string _conversationSearch = "";
        public string ConversationSearch
        {
            get => _conversationSearch;
            set
            {
                _conversationSearch = value;
                OnPropertyChanged();
                _ = LoadConversationsAsync();
            }
        }

        private string _newMessageBody = string.Empty;
        public string NewMessageBody
        {
            get => _newMessageBody;
            set
            {
                if (_newMessageBody == value) return;
                _newMessageBody = value;
                OnPropertyChanged();
            }
        }

        private void StartMessagesPolling()
        {
            _messagesPollCts?.Cancel();
            _messagesPollCts = null;

            if (SelectedRouter == null || SelectedConversation == null)
                return;

            var cts = new CancellationTokenSource();
            _messagesPollCts = cts;

            _ = MessagesPollLoopAsync(cts.Token);
        }

        private async Task MessagesPollLoopAsync(CancellationToken token)
        {
            // Do NOT reload here – LoadMessagesAsync was already called on selection

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                if (token.IsCancellationRequested)
                    break;

                if (SelectedRouter == null || SelectedConversation == null)
                    continue;

                if (string.IsNullOrEmpty(_lastMessageTs))
                    continue; // nothing loaded yet – wait for manual load

                var routerId = SelectedRouter.RouterId;
                var slot = SelectedRouter.Interface;
                var peer = SelectedConversation.Peer;

                var newItems = await _api.GetMessagesAsync(
                    routerId,
                    slot,
                    peer,
                    limit: 50,
                    sinceTs: _lastMessageTs
                );

                if (newItems == null || newItems.Count == 0)
                    continue;

                // Backend returns DESC; we want chronological append
                newItems.Reverse();

                foreach (var m in newItems)
                {
                    // Always advance last timestamp, even if we don't show the bubble
                    _lastMessageTs = m.Ts;

                    // 🔹 Skip outbound messages in poll:
                    // we already created an optimistic bubble in SendAsync,
                    // so don't add them again.
                    if (string.Equals(m.Direction, "out", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // 🔹 Extra safety: skip if a message with the same Id already exists
                    bool exists = false;
                    foreach (var existing in Messages)
                    {
                        if (existing.Id == m.Id)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (exists)
                        continue;

                    Messages.Add(m);
                }
            }
        }

        public MainViewModel(string baseUrl)
        {
            _api = new ApiClient(baseUrl);
        }

        public async Task InitializeAsync()
        {
            var routers = await _api.GetRoutersAsync();

            // We are still on the UI thread after await, so we can update collections directly
            Routers.Clear();
            foreach (var r in routers) Routers.Add(r);

            if (Routers.Count > 0)
                SelectedRouter = Routers[0];
        }

        private async Task LoadConversationsAsync()
        {
            Messages.Clear();
            SelectedConversation = null;

            if (SelectedRouter == null) return;

            var routerId = SelectedRouter.RouterId;
            var slot = SelectedRouter.Interface;

            var items = await _api.GetConversationsAsync(routerId, slot, ConversationSearch);

            Conversations.Clear();
            foreach (var c in items) Conversations.Add(c);

            if (Conversations.Count > 0)
                SelectedConversation = Conversations[0];
        }

        private async Task LoadMessagesAsync()
        {
            Messages.Clear();

            if (SelectedRouter == null || SelectedConversation == null) return;

            var routerId = SelectedRouter.RouterId;
            var slot = SelectedRouter.Interface;
            var peer = SelectedConversation.Peer;

            var items = await _api.GetMessagesAsync(routerId, slot, peer, limit: 50);

            // API returns DESC; we want oldest->newest
            items.Reverse();

            Messages.Clear();
            foreach (var m in items)
                Messages.Add(m);

            if (Messages.Count > 0)
                _lastMessageTs = Messages[Messages.Count - 1].Ts;
            else
                _lastMessageTs = null;
        }

        public string ComposerText
        {
            get => _composerText;
            set { _composerText = value; OnPropertyChanged(); }
        }
        private string _composerText = "";

        public bool IsSending
        {
            get => _isSending;
            set { _isSending = value; OnPropertyChanged(); }
        }
        private bool _isSending;

        public async Task SendAsync()
        {
            var text = NewMessageBody?.Trim();
            if (string.IsNullOrEmpty(text)) return;
            if (SelectedRouter == null || SelectedConversation == null) return;

            var routerId = SelectedRouter.RouterId;
            var slot = SelectedRouter.Interface;
            var peer = SelectedConversation.Peer;

            // optimistic bubble
            var localId = $"local:{Guid.NewGuid()}";
            var nowTs = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var tempMsg = new MessageItem
            {
                Id = localId,
                RouterId = routerId,
                Slot = slot,
                Peer = peer,
                Direction = "out",
                Body = text,
                Ts = nowTs,
                Status = "pending"   // we'll use this for StatusLabel
            };

            Messages.Add(tempMsg);
            _lastMessageTs = tempMsg.Ts;
            NewMessageBody = string.Empty;

            try
            {
                // assumes you already have this API method
                var resp = await _api.SendSmsAsync(routerId, slot, peer, text);

                if (!resp.Ok)
                {
                    tempMsg.Status = "failed";
                    tempMsg.LastError = resp.Status ?? "Send failed";
                    // notify bindings that message object changed
                    OnPropertyChanged(nameof(Messages));
                    return;
                }

                // we now know the real outbox id + status from backend
                tempMsg.Id = $"outbox:{resp.OutboxId}";
                tempMsg.Status = resp.Status ?? "pending";
                tempMsg.OutboxId = resp.OutboxId;

                // optional: start a separate watcher for this outbox id
                // to flip Status from pending -> sent/failed without reloading chat
            }
            catch
            {
                tempMsg.Status = "failed";
                OnPropertyChanged(nameof(Messages));
            }
        }



        private async Task TrackOutboxStatusAsync(MessageItem msg, long outboxId)
        {
            // poll up to ~60s
            var deadline = DateTime.UtcNow.AddSeconds(60);

            while (DateTime.UtcNow < deadline)
            {
                OutboxStatusResponse status;
                try
                {
                    status = await _api.GetOutboxStatusAsync(outboxId);
                }
                catch (Exception ex)
                {
                    // transient error -> wait and retry
                    msg.LastError = ex.Message;
                    await Task.Delay(2000);
                    continue;
                }

                msg.Status = status.Status;
                msg.Attempts = status.Attempts;
                msg.MaxAttempts = status.MaxAttempts;
                msg.ProviderId = status.ProviderId;
                msg.LastError = status.LastError;

                if (!string.IsNullOrEmpty(status.DeliveredAt))
                    msg.Ts = status.DeliveredAt;
                else if (!string.IsNullOrEmpty(status.SentAt))
                    msg.Ts = status.SentAt;

                if (string.Equals(status.Status, "sent", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(status.Status, "failed", StringComparison.OrdinalIgnoreCase))
                {
                    // done
                    return;
                }

                await Task.Delay(1500);
            }

            // timeout
            if (!msg.IsFailed && !string.Equals(msg.Status, "sent", StringComparison.OrdinalIgnoreCase))
            {
                msg.Status = "failed";
                if (string.IsNullOrEmpty(msg.LastError))
                    msg.LastError = "Timed out waiting for modem";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}