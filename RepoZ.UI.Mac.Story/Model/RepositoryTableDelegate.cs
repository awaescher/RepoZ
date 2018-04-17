using System;
using AppKit;

namespace RepoZ.UI.Mac.Story.Model
{
	public class RepositoryTableDelegate : NSTableViewDelegate
	{
		private const string CellIdentifier = "RepositoryCell";

		private RepositoryTableDataSource DataSource;

		public RepositoryTableDelegate(NSTableView tableView, RepositoryTableDataSource datasource)
		{
			this.DataSource = datasource;
		}

		public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			// This pattern allows you reuse existing views when they are no-longer in use.
			// If the returned view is null, you instance up a new view
			// If a non-null view is returned, you modify it enough to reflect the new data
			NSTextField view = (NSTextField)tableView.MakeView(CellIdentifier, this);
			if (view == null)
			{
				view = new NSTextField();
				view.Identifier = CellIdentifier;
				view.BackgroundColor = NSColor.Clear;
				view.Bordered = false;
				view.Selectable = false;
				view.Editable = false;
			}

			// Setup view based on the column selected
			switch (tableColumn.Title)
			{
				case "Repository":
					view.StringValue = DataSource.Repositories[(int)row].Name;
					break;
				case "Status":
					view.StringValue = DataSource.Repositories[(int)row].BranchWithStatus;
					break;
			}

			return view;
		}
	}
}
