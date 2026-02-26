using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RouterSimChat
{
    public class StaffLogin: INotifyPropertyChanged
    {
        public string staffid { get; set; }
        public int router_id { get; set; }
        private string _route_name;
        public string route_name {
            get => _route_name;
            set
            {
                if (_route_name != value)
                {
                    _route_name = value;
                    OnPropertyChanged(nameof(route_name));
                    OnPropertyChanged(nameof(appsName));
                }
            }
        }
        public string appsName => $"Router / SIM -  {staffid} • {route_name}";


        private RouteAgent _routeDevice;
        public RouteAgent RouteDevice
        {
            get => _routeDevice;
            set
            {
                if (_routeDevice != value)
                {
                    _routeDevice = value;
                    OnPropertyChanged(nameof(RouteDevice));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
