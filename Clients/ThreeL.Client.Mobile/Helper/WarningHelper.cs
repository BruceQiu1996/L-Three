using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace ThreeL.Client.Mobile.Helper
{
    public class WarningHelper
    {
        public async Task Warning(string message) 
        {
            ToastDuration duration = ToastDuration.Short;
            double fontSize = 14;
            var toast = Toast.Make(message, duration, fontSize);

            await toast.Show();
        }
    }
}
