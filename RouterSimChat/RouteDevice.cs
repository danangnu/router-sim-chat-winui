using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterSimChat
{
    public class RouteDevice : INotifyPropertyChanged
    {
        public int router_id { get; set; }
        public string name { get; set; }
        public string hostname { get; set; } = string.Empty;
        public string call_interface { get; set; }
        public string status { get; set; }
        public string sim { get; set; } = string.Empty;
        public Brush StatusBrush_color { get; set; }
        public string checkStr { get; set; }
        public bool isUp { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    //StatusBrush = _isActive
                    //    ? new SolidColorBrush(Colors.LightBlue)
                    //    : new SolidColorBrush(Colors.White);

                    OnPropertyChanged(nameof(IsActive));
                    OnPropertyChanged(nameof(CardBrush));
                    //OnPropertyChanged(nameof(IsActive));
                }
            }
        }
        public Brush CardBrush =>
        IsActive
            ? new SolidColorBrush(Colors.DarkBlue)
            : new SolidColorBrush(Colors.DarkGray);



        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
