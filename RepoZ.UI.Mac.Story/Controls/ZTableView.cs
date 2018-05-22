using System;
using System.Collections.Generic;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;

namespace RepoZ.UI.Mac.Story.Controls
{
    [Register("ZTableView")]
    [DesignTimeVisible(true)]
    public class ZTableView : NSTableView
    {
        public event EventHandler<nint> RepositoryActionRequested;

        public ZTableView(NSCoder coder) : base(coder)
        {
        }

        public ZTableView(CGRect frameRect) : base(frameRect)
        {
        }

        protected ZTableView(NSObjectFlag t) : base(t)
        {
        }

        protected internal ZTableView(IntPtr handle) : base(handle)
        {
        }

        public override void MouseDown(NSEvent theEvent)
        {
            base.MouseDown(theEvent);

            var locationInView = this.ConvertPointToView(theEvent.LocationInWindow, null);
            var row = GetRow(locationInView);
            if (row > -1)
                RepositoryActionRequested?.Invoke(this, row);
        }

        public override void KeyDown(NSEvent theEvent)
        {
            if (SelectedRow > -1 && SelectorKeys.Contains(theEvent.KeyCode))
            {
                RepositoryActionRequested?.Invoke(this, SelectedRow);
                return; // do not call base to avoid the system beeping
            }

            base.KeyDown(theEvent);
        }

        protected List<ushort> SelectorKeys { get; } = new List<ushort> { (ushort)NSKey.Space, (ushort)NSKey.Return, (ushort)NSKey.KeypadEnter };
    }
}
