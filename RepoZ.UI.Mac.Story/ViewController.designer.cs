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
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSTableView RepositoryTable { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (RepositoryTable != null) {
				RepositoryTable.Dispose ();
				RepositoryTable = null;
			}
		}
	}
}
