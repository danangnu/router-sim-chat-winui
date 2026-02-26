using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Input;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Linq;
using Windows.System;

namespace RouterSimChat
{
    public sealed partial class MainPage : Page
    {
        public ChatViewModel ViewModel { get; set; } = new ChatViewModel();

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
            this.Loaded += MainPage_Loaded;

            // 🔹 Auto-scroll when new messages arrive
            if (ViewModel.Messages is INotifyCollectionChanged coll)
            {
                coll.CollectionChanged += Messages_CollectionChanged;
            }
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.IsLoading = true;

            try
            {
                await Task.WhenAll(
                    // add all the initial API calls here
                    // ViewModel.LoadLastMessagesAsync(48),
                    // ViewModel.LoadContact(),
                    ViewModel.loadStaffTrackit()
                );

                // scroll once after initial load
                DispatcherQueue.TryEnqueue(async () =>
                {
                    await Task.Delay(50);
                    ChatScrollViewer.ChangeView(
                        null,
                        ChatScrollViewer.ScrollableHeight,
                        null
                    );
                });
            }
            finally
            {
                ViewModel.IsLoading = false;
            }
        }

        private async Task ScrollToBottomAsync()
        {
            // small delay so layout/height is updated
            await Task.Delay(50);

            if (ChatScrollViewer != null)
            {
                ChatScrollViewer.ChangeView(
                    null,
                    ChatScrollViewer.ScrollableHeight,
                    null
                );
            }
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // We only care about new items added / reset
            if (e.Action != NotifyCollectionChangedAction.Add &&
                e.Action != NotifyCollectionChangedAction.Reset)
                return;

            // Ensure we run on UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                _ = ScrollToBottomAsync();
            });
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // closes app
            (Application.Current as App)?.Exit();
        }

        private async void RouterList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is RouteDevice device)
            {
                await ViewModel.SelectDeviceAsync(device);
            }
        }

        private async void chat_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ChatLastMessage msg)
            {
                await ViewModel.OpenChatAsync(msg);
                await Task.Delay(50);
                ChatScrollViewer.ChangeView(
                    null,
                    ChatScrollViewer.ScrollableHeight,
                    null
                );
            }
            // 🟢 Kalau yang diklik adalah contact baru
            else if (e.ClickedItem is Contact contact)
            {
                ViewModel.OpenEmptyChat(contact);
            }
        }

        // 🔹 Shared send logic (used by button + Enter key)
        private async Task SendCurrentMessageAsync()
        {
            await ViewModel.SendSmsAsync();

            await Task.Delay(50);
            ChatScrollViewer.ChangeView(
                null,
                ChatScrollViewer.ScrollableHeight,
                null
            );
        }

        // 🔹 Send button click → use shared method
        private async void SendButton_click(object sender, RoutedEventArgs e)
        {
            await SendCurrentMessageAsync();
        }

        // 🔹 Enter on TextBox → send message
        private async void MessageTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter)
                return;

            // prevent TextBox from inserting a newline
            e.Handled = true;

            await SendCurrentMessageAsync();
        }

        private async void OpenTrackIT_click(object sender, RoutedEventArgs e)
        {
            await ViewModel.opencardTI();
        }

        private async void refreshmsg_click(object sender, RoutedEventArgs e)
        {
            await ViewModel.refreshMessagesAsync();
            await Task.Delay(50);
        }

        private async void clearMessage(object sender, RoutedEventArgs e)
        {
            ViewModel.Messages.Clear();
            ViewModel.chatDetails.Clear();
        }

        public async Task OpenChatFromToast(string sender)
        {
            await ViewModel.OpenChatBySenderAsync(sender);

            await Task.Delay(50);

            ChatScrollViewer.ChangeView(
                null,
                ChatScrollViewer.ScrollableHeight,
                null
            );
        }
    }
}