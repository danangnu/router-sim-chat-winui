using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterSimChat
{
    public enum ChatBoxType
    {
        outbound,
        inbound
    }

    public class ChatMessage : ChatItem
    {
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public String Box { get; set; }
        public string sortTime => Time.ToString("HH:mm");
        public String Status { get; set; }
        public string StatusIcon =>
        Status switch
        {
            "pending" => "⏳",
            "processing" => "✓",
            "sent" => "✓✓",
            "failed" => "❗",
            _ => ""
        };

    }
}
