using System.Windows;
using System.Windows.Controls;
using ThreeL.Client.Win.ViewModels;
using ThreeL.Client.Win.ViewModels.Messages;

namespace ThreeL.Client.Win.Converters
{
    public class MessageDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var fe = container as FrameworkElement;
            var obj = item as MessageViewModel;
            DataTemplate dt = null;
            if (obj != null && fe != null)
            {
                if (obj is TimeMessage)
                    dt = fe.FindResource("time") as DataTemplate;
                else if (obj is TextMessage && obj.FromSelf)
                    dt = fe.FindResource("txtSender") as DataTemplate;
                else if (obj is TextMessage && !obj.FromSelf)
                    dt = fe.FindResource("txtReceiver") as DataTemplate;
                else if (obj is ImageMessage && obj.FromSelf)
                    dt = fe.FindResource("imageSender") as DataTemplate;
                else if (obj is ImageMessage && !obj.FromSelf)
                    dt = fe.FindResource("imageReceiver") as DataTemplate;
            }

            return dt;
        }
    }
}
