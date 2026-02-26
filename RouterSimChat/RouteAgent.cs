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
    public class RouteAgent
    {
        public int router_id { get; set; }
        private DateTime _lastHeartbeat;
        public DateTime last_heartbeat {
            get => _lastHeartbeat;
            set
            {
                if (_lastHeartbeat != value)
                {
                    _lastHeartbeat = value;
                    OnPropertyChanged(nameof(last_heartbeat));
                    OnPropertyChanged(nameof(IsActive));
                    OnPropertyChanged(nameof(isOnline));
                    OnPropertyChanged(nameof(status));
                }
            }
        }
        public string status =>
        IsActive
          ? "Online"
          : "Offline";


        // ⬇️ dihitung otomatis
        public bool IsActive =>
            DateTime.Now - last_heartbeat <= TimeSpan.FromMinutes(2);

        public Brush isOnline =>
            IsActive
                ? new SolidColorBrush(Colors.LightGreen)
                : new SolidColorBrush(Colors.Red);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
