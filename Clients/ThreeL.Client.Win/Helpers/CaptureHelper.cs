using System;
using System.Runtime.InteropServices;

namespace ThreeL.Client.Win.Helpers
{
    public class CaptureHelper
    {
        [DllImport("user32.dll")]
        private static extern uint SetWindowDisplayAffinity(IntPtr hwnd, uint dwAffinity);

        public bool HasProtect
        {
            get { return isProtect; }
        }

        private bool isProtect = false;

        public void Protect(IntPtr handle)
        {
            SetWindowDisplayAffinity(handle, 3);
            isProtect = true;
        }

        public void UnProtect(IntPtr handle)
        {
            if (isProtect)
            {
                SetWindowDisplayAffinity(handle, 0);
                isProtect = false;
            }
        }
    }
}
