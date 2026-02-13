using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RouterSimChat.Models;

namespace RouterSimChat
{
    public sealed class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? InboundTemplate { get; set; }
        public DataTemplate? OutboundTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is MessageItem msg &&
                msg.Direction != null &&
                msg.Direction.ToLowerInvariant() == "out")
            {
                return OutboundTemplate ?? InboundTemplate!;
            }

            return InboundTemplate ?? base.SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
            => SelectTemplateCore(item);
    }
}