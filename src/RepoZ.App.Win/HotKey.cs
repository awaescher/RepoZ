namespace RepoZ.App.Win
{
    using System;
    using System.Windows.Interop;
    using System.Runtime.InteropServices;
    using System.Windows;

    internal class HotKey
    {
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        public const uint VK_R = 0x52;
        public const uint VK_Z = 0x5A;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CTRL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        private IntPtr _handle;
        private Action _hotKeyActionToCall;
        private HwndSource _source;
        private readonly int _id;

        public HotKey(int id)
        {
            _id = id;
        }

        public void Register(Window window, uint key, uint modifiers, Action hotKeyActionToCall)
        {
            var helper = new WindowInteropHelper(window);
            _handle = helper.EnsureHandle();
            _hotKeyActionToCall = hotKeyActionToCall;

            _source = HwndSource.FromHwnd(_handle);
            _source?.AddHook(HwndHook);

            if (!RegisterHotKey(_handle, _id, modifiers, key))
            {
                // handle error
            }
        }

        public void Unregister()
        {
            UnregisterHotKey(_handle, _id);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    if (wParam.ToInt32() == _id)
                    {
                        _hotKeyActionToCall?.Invoke();
                        handled = true;
                    }

                    break;
            }

            return IntPtr.Zero;
        }
    }
}