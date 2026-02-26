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
        private string _statusHeartbeat;
        public string statusHeartbeat
        {
            get => _statusHeartbeat;
            set
            {
                if (_statusHeartbeat != value)
                {
                    _statusHeartbeat = value;
                    OnPropertyChanged(nameof(statusHeartbeat));
                }
            }
        }
        public string sim { get; set; } = string.Empty;
        private Brush _StatusBrush_color;
        public Brush StatusBrush_color
        {
            get => _StatusBrush_color;
            set
            {
                if (_StatusBrush_color != value)
                {
                    _StatusBrush_color = value;
                    OnPropertyChanged(nameof(StatusBrush_color));
                }
            }
        }
        private string _checkStr;
        public string checkStr
        {
            get => _checkStr;
            set
            {
                if (_checkStr != value)
                {
                    _checkStr = value;
                    OnPropertyChanged(nameof(checkStr));
                }
            }
        }
        public DateTime last_heartbeat { get; set; }
        private bool _isUp;
        public bool isUp {
            get => _isUp;
            set
            {
                if (_isUp != value)
                {
                    _isUp = value;
                    OnPropertyChanged(nameof(isUp));
                }
            }
        }

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
