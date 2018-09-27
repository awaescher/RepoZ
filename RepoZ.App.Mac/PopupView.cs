using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using RepoZ.Api.Git;
using RepoZ.App.Mac.Model;

namespace RepoZ.App.Mac
{
    public partial class PopupView : AppKit.NSView
    {
        #region Constructors

        // Called when created from unmanaged code
        public PopupView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public PopupView(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

    }
}
