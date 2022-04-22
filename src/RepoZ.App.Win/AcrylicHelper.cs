namespace RepoZ.App.Win
{
    using System;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;

    public static class AcrylicHelper
    {
        public static void EnableBlur(Visual visual)
        {
            var hwnd = (HwndSource)PresentationSource.FromVisual(visual);
            if (hwnd != null)
            {
                EnableBlur(hwnd.Handle);
            }
        }

        public static void EnableBlur(IntPtr hwnd)
        {
            WindowsCompositionHelper.EnableBlur(hwnd);
        }
    }
}