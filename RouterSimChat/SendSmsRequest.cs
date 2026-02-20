using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterSimChat
{
    public class SendSmsRequest
    {
        public int router_id { get; set; }
        public string slot { get; set; }
        public string peer { get; set; }
        public string body { get; set; }
    }
}
