using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;

namespace RepoZ.App.Mac.Controls
{
    [Register("ZTableView")]
    [DesignTimeVisible(true)]
    public class ZTableView : NSTableView
    {
        public event EventHandler<nint> RepositoryActionRequested;
        public event EventHandler<ContextMenuArguments> PrepareContextMenu;

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

            var isDoubleClick = theEvent.ClickCount > 1;

            if (isDoubleClick)
            {
                var row = GetRowByMouseEventArgs(theEvent);
                if (row > -1)
                    RepositoryActionRequested?.Invoke(this, row);
            }
        }

        public override void RightMouseDown(NSEvent theEvent)
        {
            base.RightMouseDown(theEvent);

            var row = GetRowByMouseEventArgs(theEvent);
            if (row > -1)
            {
                var selectedRowIndexes = SelectedRows.AsEnumerable().ToList();

                if (!selectedRowIndexes.Contains((nuint)row))
                {
                    var extendSelection = UiStateHelper.CommandKeyDown;
                    SelectRow(row, extendSelection);
                    selectedRowIndexes = SelectedRows.AsEnumerable().ToList();
                }

                var menuItems = new List<NSMenuItem>();
                PrepareContextMenu?.Invoke(this, new ContextMenuArguments(menuItems, selectedRowIndexes));

                if (menuItems.Any())
                {
                    var menu = new NSMenu { AutoEnablesItems = false };

                    foreach (var item in menuItems)
                        menu.AddItem(item);

                    var locationInView = this.ConvertPointToView(theEvent.LocationInWindow, null);
                    locationInView.X -= 26;

                    menu.PopUpMenu(null, locationInView, this);
                }
            }
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

        private nint GetRowByMouseEventArgs(NSEvent args)
        {
            var locationInView = this.ConvertPointToView(args.LocationInWindow, null);
            return GetRow(locationInView);
        }

        protected List<ushort> SelectorKeys { get; } = new List<ushort> { (ushort)NSKey.Space, (ushort)NSKey.Return, (ushort)NSKey.KeypadEnter };
    }

    public class ContextMenuArguments
    {
        public ContextMenuArguments(List<NSMenuItem> menuItems, List<nuint> rows)
        {
            MenuItems = menuItems ?? throw new ArgumentNullException(nameof(menuItems));
            Rows = rows;
        }

        public List<nuint> Rows { get; }

        public List<NSMenuItem> MenuItems { get; }
    }
}
