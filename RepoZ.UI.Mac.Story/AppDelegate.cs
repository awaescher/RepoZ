using AppKit;
using Foundation;

namespace RepoZ.UI.Mac.Story
{
    [Register("AppDelegate")]
    public partial class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            // Insert code here to initialize your application
            var statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(70);
            statusItem.Menu = SystemTrayStatusMenu;
            statusItem.Title = SystemTrayStatusMenu.Title;
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
