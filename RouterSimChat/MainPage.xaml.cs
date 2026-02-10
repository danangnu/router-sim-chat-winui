using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RouterSimChat.ViewModels;

namespace RouterSimChat
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public MainPage()
        {
            this.InitializeComponent();

            ViewModel = new MainViewModel(
                ui: Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread(),
                baseUrl: "http://127.0.0.1:8080"
            );

            DataContext = ViewModel;

            _ = ViewModel.InitializeAsync();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current as App)?.Exit();
        }
    }
}