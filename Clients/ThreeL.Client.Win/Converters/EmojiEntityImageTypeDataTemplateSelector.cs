using System.Windows;
using System.Windows.Controls;
using ThreeL.Client.Win.Models;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Client.Win.Converters
{
    internal class EmojiEntityImageTypeDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var fe = container as FrameworkElement;
            var obj = item as EmojiEntity;
            DataTemplate dt = null;
            if (obj != null && fe != null)
            {
                dt = fe.FindResource("network") as DataTemplate;
            }

            return dt;
        }
    }
}
