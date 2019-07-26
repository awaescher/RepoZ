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
	[Register ("PopupViewController")]
	partial class PopupViewController
	{
		[Outlet]
		AppKit.NSTextField lblNoRepositories { get; set; }

		[Outlet]
		AppKit.NSButton MenuButton { get; set; }

		[Outlet]
		AppKit.NSTextField NameTextField { get; set; }

		[Outlet]
		RepoZ.App.Mac.Controls.ZTableView RepoTab { get; set; }

		[Outlet]
		RepoZ.App.Mac.Controls.ZSearchField SearchBox { get; set; }

		[Outlet]
		AppKit.NSButton UpdateButton { get; set; }

		[Action ("MenuButton_Click:")]
		partial void MenuButton_Click (Foundation.NSObject sender);

		[Action ("SearchChanged:")]
		partial void SearchChanged (Foundation.NSObject sender);

		[Action ("StatusLabel:")]
		partial void StatusLabel (Foundation.NSObject sender);

		[Action ("UpdateButton_Click:")]
		partial void UpdateButton_Click (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (lblNoRepositories != null) {
				lblNoRepositories.Dispose ();
				lblNoRepositories = null;
			}

			if (MenuButton != null) {
				MenuButton.Dispose ();
				MenuButton = null;
			}

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

			if (UpdateButton != null) {
				UpdateButton.Dispose ();
				UpdateButton = null;
			}
		}
	}
}
