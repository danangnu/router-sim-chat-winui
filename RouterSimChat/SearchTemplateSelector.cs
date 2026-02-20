using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterSimChat
{
    public class SearchTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ChatTemplate { get; set; }
        public DataTemplate ContactTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is ChatLastMessage)
                return ChatTemplate;

            if (item is Contact)
                return ContactTemplate;

            return base.SelectTemplateCore(item);
        }
    }
}
