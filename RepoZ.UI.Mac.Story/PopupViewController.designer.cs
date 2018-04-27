// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace RepoZ.UI.Mac.Story
{
	[Register ("PopupViewController")]
	partial class PopupViewController
	{
		[Outlet]
		AppKit.NSTextField NameTextField { get; set; }

		[Outlet]
		AppKit.NSTableView RepoTab { get; set; }

		[Outlet]
		AppKit.NSSearchField SearchBox { get; set; }

		[Action ("SearchChanged:")]
		partial void SearchChanged (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (NameTextField != null) {
				NameTextField.Dispose ();
				NameTextField = null;
			}

			if (RepoTab != null) {
				RepoTab.Dispose ();
				RepoTab = null;
			}

			if (SearchBox != null) {
				SearchBox.Dispose ();
				SearchBox = null;
			}
		}
	}
}
