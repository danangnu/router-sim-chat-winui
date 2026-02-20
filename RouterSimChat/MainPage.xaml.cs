using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Linq;

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
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.WhenAll(
                 //ViewModel.loadStaffTrackit(),
                 //ViewModel.LoadLastMessagesAsync(48),
                 //ViewModel.LoadContact(),
                 ViewModel.loadStaffTrackit()
             );

            // ⬇ scroll sekali, setelah data beres
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
                //await ViewModel.MarkAsReadAsync(msg);
                //await ViewModel.SelectMessageAsync(msg);
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
       
        private async void SendButton_click(object sender, RoutedEventArgs e)
        {
            await ViewModel.SendSmsAsync();
            await Task.Delay(50);
            ChatScrollViewer.ChangeView(
                   null,
                   ChatScrollViewer.ScrollableHeight,
                   null
               );
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
        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    ViewModel.StartAutoRefresh();
        //}
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