using System;
using AppKit;
using Foundation;

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
            // If the returned view is null, you instance up a new view.
            // If a non-null view is returned, you modify it enough to reflect the new data.
            var cell = tableView.MakeView(CellIdentifier, this);

            var repository = DataSource.Repositories[(int)row];

            var RepositoryLabel = (cell.Subviews[1] as NSTextField);
            var PathLabel = (cell.Subviews[2] as NSTextField);
            var StatusLabel = (cell.Subviews[3] as NSTextField);

            RepositoryLabel.StringValue = repository.Name;
            PathLabel.StringValue = repository.Path;
            StatusLabel.StringValue = repository.Status;

            return cell;
        }
    }
}
