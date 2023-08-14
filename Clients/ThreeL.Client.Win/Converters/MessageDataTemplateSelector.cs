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
                if (obj is TimeMessageViewModel)
                    dt = fe.FindResource("time") as DataTemplate;
                else if (obj is TextMessageViewModel && obj.FromSelf)
                    dt = fe.FindResource("txtSender") as DataTemplate;
                else if (obj is TextMessageViewModel && !obj.FromSelf)
                    dt = fe.FindResource("txtReceiver") as DataTemplate;
                else if (obj is ImageMessageViewModel && obj.FromSelf)
                    dt = fe.FindResource("imageSender") as DataTemplate;
                else if (obj is ImageMessageViewModel && !obj.FromSelf)
                    dt = fe.FindResource("imageReceiver") as DataTemplate;
                else if (obj is FileMessageViewModel && obj.FromSelf)
                    dt = fe.FindResource("fileSender") as DataTemplate;
                else if (obj is FileMessageViewModel && !obj.FromSelf)
                    dt = fe.FindResource("fileReceiver") as DataTemplate;
            }

            return dt;
        }
    }
}
