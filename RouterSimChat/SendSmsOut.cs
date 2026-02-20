using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterSimChat
{
    public class SendSmsOut
    {
        public bool ok { get; set; }
        public int router_id { get; set; }
        public string peer { get; set; }
        public string status { get; set; }
    }
}
