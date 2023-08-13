using HandyControl.Controls;
using HandyControl.Data;

namespace ThreeL.Client.Win.Helpers
{
    public class GrowlHelper
    {
        public void Info(string message)
        {
            Growl.InfoGlobal(new GrowlInfo()
            {
                Message = message,
                ShowDateTime = false,
                ShowCloseButton = false,
                StaysOpen = false,
                WaitTime = 3
            });
        }

        public void Warning(string message)
        {
            Growl.WarningGlobal(new GrowlInfo()
            {
                Message = message,
                ShowDateTime = false,
                ShowCloseButton = false,
                StaysOpen = false,
                WaitTime = 3
            });
        }
    }
}
