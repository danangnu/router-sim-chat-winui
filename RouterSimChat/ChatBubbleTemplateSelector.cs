using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace RouterSimChat
{
    public class ChatBubbleTemplateSelector : DataTemplateSelector
    {
        public DataTemplate InboundTemplate { get; set; }
        public DataTemplate OutboundTemplate { get; set; }
        public DataTemplate DateTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            //Debug.WriteLine("🔥 ChatBubbleTemplateSelector dipanggil");


            if (item is ChatDateSeparator)
                return DateTemplate;

            var msg = item as ChatMessage;

            if (msg.Box == "inbound")
                return InboundTemplate;

            return OutboundTemplate;

            //return base.SelectTemplateCore(item);
            //if (msg.Box == "inbound")
            //    return InboundTemplate;

            //return OutboundTemplate;


        }

        // WAJIB override ini juga di WinUI 3
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }
}
