using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;

namespace RepoZ.UI.Mac.Story.Controls
{
    [Register("ZSearchField")]
    [DesignTimeVisible(true)]
    public class ZSearchField : NSSearchField
    {
        public ZSearchField(NSCoder coder) : base(coder)
        {
        }

        public ZSearchField(CGRect frameRect) : base(frameRect)
        {
        }

        protected ZSearchField(NSObjectFlag t) : base(t)
        {
        }

        protected internal ZSearchField(IntPtr handle) : base(handle)
        {
        }

        public override void KeyDown(NSEvent theEvent)
        {
            base.KeyDown(theEvent);
        }
    }
}
