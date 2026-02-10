using Microsoft.UI.Dispatching;
using RouterSimChat.Models;
using RouterSimChat.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RouterSimChat.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _api;
        private readonly DispatcherQueue _ui;

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
                _ = LoadMessagesAsync();
            }
        }

        private string _routerSearch = "";
        public string RouterSearch
        {
            get => _routerSearch;
            set { _routerSearch = value; OnPropertyChanged(); /* optional filter later */ }
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

        public MainViewModel(DispatcherQueue ui, string baseUrl)
        {
            _ui = ui;
            _api = new ApiClient(baseUrl);
        }

        public async Task InitializeAsync()
        {
            var routers = await _api.GetRoutersAsync();
            if (routers.Count > 0)
                System.Diagnostics.Trace.WriteLine($"[VM] routers[0].RouterId={routers[0].RouterId}, Interface={routers[0].Interface}");

            _ui.TryEnqueue(() =>
            {
                Routers.Clear();
                foreach (var r in routers) Routers.Add(r);

                // auto select first
                if (Routers.Count > 0)
                    SelectedRouter = Routers[0];
            });
        }

        private async Task LoadConversationsAsync()
        {
            Messages.Clear();
            SelectedConversation = null;

            if (SelectedRouter == null) return;

            var routerId = SelectedRouter.RouterId;
            var slot = SelectedRouter.Interface;

            var items = await _api.GetConversationsAsync(routerId, slot, ConversationSearch);
            System.Diagnostics.Debug.WriteLine($"SelectedRouter.RouterId={SelectedRouter?.RouterId}, Slot={SelectedRouter?.Interface}");
            _ui.TryEnqueue(() =>
            {
                Conversations.Clear();
                foreach (var c in items) Conversations.Add(c);

                if (Conversations.Count > 0)
                    SelectedConversation = Conversations[0];
            });
        }

        private async Task LoadMessagesAsync()
        {
            Messages.Clear();

            if (SelectedRouter == null || SelectedConversation == null) return;

            var routerId = SelectedRouter.RouterId;
            var slot = SelectedRouter.Interface;
            var peer = SelectedConversation.Peer;

            var items = await _api.GetMessagesAsync(routerId, slot, peer, limit: 50);

            // API returns DESC; for chat UI we want oldest->newest
            items.Reverse();

            _ui.TryEnqueue(() =>
            {
                Messages.Clear();
                foreach (var m in items) Messages.Add(m);
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}