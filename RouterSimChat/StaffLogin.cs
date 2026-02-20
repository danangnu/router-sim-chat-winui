using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RouterSimChat
{
    public class StaffLogin
    {
        public string staffid { get; set; }
        public int router_id { get; set; }
        public string route_name { get; set; }
        public string appsName => $"Router / SIM -  {staffid} • {route_name}";

    }
}
