using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.Graphics;
using WinRT.Interop;

namespace RouterSimChat
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Navigate to your MainPage
            RootFrame.Navigate(typeof(MainPage));

            // Set title + window size (WinUI 3 way)
            TrySetWindowSize(1200, 720);
            Title = "Router-SIM Chat";
        }

        private void TrySetWindowSize(int width, int height)
        {
            // Get AppWindow from this Window
            var hwnd = WindowNative.GetWindowHandle(this);
            var winId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(winId);

            // Resize
            appWindow.Resize(new SizeInt32(width, height));
        }
        public async Task OpenChat(string sender)
        {
            if (Content is Frame frame)
            {
                if (frame.Content is not MainPage)
                {
                    frame.Navigate(typeof(MainPage));
                }

                if (frame.Content is MainPage page)
                {
                    await page.OpenChatFromToast(sender);
                }
            }

            this.Activate();
        }
    }
}