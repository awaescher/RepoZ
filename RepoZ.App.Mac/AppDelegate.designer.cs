// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace RepoZ.App.Mac
{
    partial class AppDelegate
    {
        [Outlet]
        AppKit.NSMenu MyMainMenu { get; set; }

        [Outlet]
        AppKit.NSMenu SystemTrayStatusMenu { get; set; }
        
        void ReleaseDesignerOutlets ()
        {
            if (SystemTrayStatusMenu != null) {
                SystemTrayStatusMenu.Dispose ();
                SystemTrayStatusMenu = null;
            }

            if (MyMainMenu != null) {
                MyMainMenu.Dispose ();
                MyMainMenu = null;
            }
        }
    }
}
