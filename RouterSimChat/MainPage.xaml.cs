using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RouterSimChat.ViewModels;

namespace RouterSimChat
{
    public sealed partial class MainPage : Page
    {
        // Initialize the VM inline – it is NEVER null after this point
        public MainViewModel ViewModel { get; } =
            new MainViewModel("http://127.0.0.1:8080");

        public MainPage()
        {
            this.InitializeComponent();

            // ❌ REMOVE this – we won't use DataContext for bindings anymore
            // DataContext = ViewModel;

            // Kick async init when page is loaded
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainPage_Loaded;
            await ViewModel.InitializeAsync();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current as App)?.Exit();
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.SendAsync();
            }
        }
    }
}