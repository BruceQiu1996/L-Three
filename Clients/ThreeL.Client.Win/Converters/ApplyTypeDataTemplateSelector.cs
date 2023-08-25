using System.Windows;
using System.Windows.Controls;
using ThreeL.Client.Win.ViewModels;

namespace ThreeL.Client.Win.Converters
{
    internal class ApplyTypeDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var fe = container as FrameworkElement;
            var obj = item as ApplyRecordViewModel;
            DataTemplate dt = null;
            if (obj != null && fe != null)
            {
                if (obj.FromSelf)
                {
                    dt = fe.FindResource("send") as DataTemplate;
                }
                else 
                {
                    dt = fe.FindResource("receive") as DataTemplate;
                }
            }

            return dt;
        }
    }
}
