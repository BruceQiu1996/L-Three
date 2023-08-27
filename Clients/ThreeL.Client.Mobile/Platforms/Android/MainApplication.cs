using Android.App;
using Android.Content.Res;
using Android.Runtime;
using Microsoft.Maui.Platform;

namespace ThreeL.Client.Mobile
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            Microsoft.Maui.Handlers.EntryHandler.Mapper.Add("RemoveBorder", (h, w) => 
            {
                h.PlatformView.Background = null;
            });
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}