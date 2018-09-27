using System;
using System.Linq;
using AppKit;
using Foundation;
using RepoZ.Api.Git;
using RepoZ.App.Mac.Controls;

namespace RepoZ.App.Mac.Model
{
    public class RepositoryTableDelegate : NSTableViewDelegate, INSTextFieldDelegate
    {
        private const string CellIdentifier = "RepositoryCell";

        public RepositoryTableDelegate(ZTableView tableView, RepositoryTableDataSource datasource, IRepositoryActionProvider repositoryActionProvider)
        {
            RepositoryActionProvider = repositoryActionProvider ?? throw new ArgumentNullException(nameof(repositoryActionProvider));

            TableView = tableView;
            DataSource = datasource;

            TableView.RepositoryActionRequested += TableView_RepositoryActionRequested;
            DataSource.CollectionChanged += ReloadTableView;
        }

		protected override void Dispose(bool disposing)
		{
            TableView.RepositoryActionRequested -= TableView_RepositoryActionRequested;
            DataSource.CollectionChanged -= ReloadTableView;

            base.Dispose(disposing);
		}

        private void ReloadTableView(object sender, EventArgs args)
        {
            this.TableView.ReloadData();
        }
                                     
		public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
        {
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view.
            // If a non-null view is returned, you modify it enough to reflect the new data.
            var cell = tableView.MakeView(CellIdentifier, this);

            var repositoryView = DataSource.GetRepositoryViewByIndex((int)row);
            if (repositoryView == null)
                return cell;

            var labels = cell.Subviews.OfType<NSTextField>().ToArray();
            var RepositoryLabel = labels.Single(l => l.Identifier == "RepositoryLabel");
            var CurrentBranchLabel = labels.Single(l => l.Identifier == "CurrentBranchLabel");
            var StatusLabel = labels.Single(l => l.Identifier == "StatusLabel");

            RepositoryLabel.StringValue = repositoryView.Name;
            RepositoryLabel.ToolTip = repositoryView.Path;
            CurrentBranchLabel.StringValue = repositoryView.CurrentBranch;
            StatusLabel.StringValue = repositoryView.Status;

            return cell;
        }

        void TableView_RepositoryActionRequested(object sender, nint rowIndex)
        {
            InvokeRepositoryAction(rowIndex);
        }

        public void InvokeRepositoryAction(nint rowIndex)
        {
            var repositoryView = DataSource.GetRepositoryViewByIndex((int)rowIndex);

            if (repositoryView == null)
                return;

            System.Diagnostics.Debug.WriteLine($"Clicked row {rowIndex} which was: {repositoryView.Name}");

            RepositoryAction action;

            if (UiStateHelper.CommandKeyDown || UiStateHelper.OptionKeyDown)
                action = RepositoryActionProvider.GetSecondaryAction(repositoryView.Repository);
            else
                action = RepositoryActionProvider.GetPrimaryAction(repositoryView.Repository);

            action?.Action?.Invoke(this, EventArgs.Empty);
        }

        public ZTableView TableView { get; }

        public RepositoryTableDataSource DataSource { get; }

        public IRepositoryActionProvider RepositoryActionProvider { get; }
    }
}