using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace RouterSimChat
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // closes app
            (Application.Current as App)?.Exit();
        }
    }
}