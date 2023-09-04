using System.Windows;
using System.Windows.Controls;
using ThreeL.Client.Win.ViewModels;
using ThreeL.Shared.SuperSocket.Metadata;

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
                if (obj.MessageType == MessageType.Time)
                    dt = fe.FindResource("time") as DataTemplate;
                if (obj.MessageType == MessageType.Tip)
                    dt = fe.FindResource("tip") as DataTemplate;
                else if (obj.MessageType == MessageType.Text && obj.FromSelf)
                    dt = fe.FindResource("txtSender") as DataTemplate;
                else if (obj.MessageType == MessageType.Text && !obj.FromSelf)
                    dt = fe.FindResource("txtReceiver") as DataTemplate;
                else if (obj.MessageType == MessageType.Image && obj.FromSelf)
                    dt = fe.FindResource("imageSender") as DataTemplate;
                else if (obj.MessageType == MessageType.Image && !obj.FromSelf)
                    dt = fe.FindResource("imageReceiver") as DataTemplate;
                else if (obj.MessageType == MessageType.File && obj.FromSelf)
                    dt = fe.FindResource("fileSender") as DataTemplate;
                else if (obj.MessageType == MessageType.File && !obj.FromSelf)
                    dt = fe.FindResource("fileReceiver") as DataTemplate;
                else if (obj.MessageType == MessageType.LoadRecord)
                    dt = fe.FindResource("loadData") as DataTemplate;
            }

            return dt;
        }
    }
}
