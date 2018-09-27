using System;
using System.Collections.Generic;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;

namespace RepoZ.App.Mac.Controls
{
    [Register("ZSearchField")]
    [DesignTimeVisible(true)]
    public class ZSearchField : NSSearchField
    {
        public event EventHandler FinishInput;

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

        public override void KeyUp(NSEvent theEvent)
        {
            base.KeyUp(theEvent);

            if (FinisherKeys.Contains(theEvent.KeyCode))
                FinishInput?.Invoke(this, EventArgs.Empty);
        }

        protected List<ushort> FinisherKeys { get; } = new List<ushort> { (ushort)NSKey.DownArrow, (ushort)NSKey.Return, (ushort)NSKey.KeypadEnter };
    }
}
