using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;
using RepoZ.Api.Common;
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
            TableView.PrepareContextMenu += TableView_PrepareContextMenu;
            DataSource.CollectionChanged += ReloadTableView;

            Humanizer = new HardcodededMiniHumanizer();
        }

		protected override void Dispose(bool disposing)
		{
            TableView.RepositoryActionRequested -= TableView_RepositoryActionRequested;
            TableView.PrepareContextMenu -= TableView_PrepareContextMenu;
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

            var labels = cell.Subviews.OfType<NSControl>().ToArray();
            var buttons = cell.Subviews.OfType<NSButton>().ToArray();
            var RepositoryLabel = labels.Single(l => l.Identifier == "RepositoryLabel");
            var CurrentBranchLabel = labels.Single(l => l.Identifier == "CurrentBranchLabel");
            var StatusLabel = buttons.Single(l => l.Identifier == "StatusLabel");

            RepositoryLabel.StringValue = repositoryView.Name;
            RepositoryLabel.ToolTip = repositoryView.Path;
            CurrentBranchLabel.StringValue = repositoryView.CurrentBranch;
            StatusLabel.Title = repositoryView.Status;
            StatusLabel.ToolTip = repositoryView.UpdateStampUtc.ToLocalTime().ToShortTimeString();
            StatusLabel.SizeToFit();
            StatusLabel.SetBoundsOrigin(new CoreGraphics.CGPoint(CurrentBranchLabel.Bounds.Left, StatusLabel.Bounds.Top));
            // would be nice, but does not update: Humanizer.HumanizeTimestamp(repositoryView.UpdateStampUtc.ToLocalTime());

            return cell;
        }

        void TableView_RepositoryActionRequested(object sender, nint rowIndex)
        {
            InvokeRepositoryAction(rowIndex);
        }

        void TableView_PrepareContextMenu(object sender, ContextMenuArguments arguments)
        {
            PrepareContextMenu(sender, arguments);
        }

        public void InvokeRepositoryAction(nint rowIndex)
        {
            var repositoryView = DataSource.GetRepositoryViewByIndex((int)rowIndex);

            if (repositoryView == null)
                return;

            RepositoryAction action;

            if (UiStateHelper.CommandKeyDown || UiStateHelper.OptionKeyDown)
                action = RepositoryActionProvider.GetSecondaryAction(repositoryView.Repository);
            else
                action = RepositoryActionProvider.GetPrimaryAction(repositoryView.Repository);

            action?.Action?.Invoke(this, EventArgs.Empty);
        }

        public void PrepareContextMenu(object sender, ContextMenuArguments arguments)
        {
            if (!arguments.Rows.Any())
                return;

            var repositories = arguments.Rows
                .Select(i => DataSource.GetRepositoryViewByIndex((int)i))
                .Where(view => view.Repository != null)
                .Select(view => view.Repository)
                .ToList();

            if (!repositories.Any())
                return;

            foreach (var action in RepositoryActionProvider.GetContextMenuActions(repositories))
            {
                var item = CreateMenuItem(sender, action, arguments);
                arguments.MenuItems.Add(item);
            }
        }

        private NSMenuItem CreateMenuItem(object sender, RepositoryAction action, ContextMenuArguments arguments)
        {
            if (action.BeginGroup)
                arguments.MenuItems.Add(NSMenuItem.SeparatorItem);

            var item = new NSMenuItem(action.Name, (s, e) => action.Action?.Invoke(s, e));
            item.Enabled = action.CanExecute;

            // this is a deferred submenu. We want to make sure that the context menu can pop up
            // fast, while submenus are not evaluated yet. We don't want to make the context menu
            // itself slow because the creation of the submenu items takes some time.
            if (action.DeferredSubActionsEnumerator != null)
            {
                var submenu = new NSMenu { AutoEnablesItems = false };
                submenu.Delegate = new DeferredInitializerDelegate(arguments.Initializers);

                arguments.Initializers.Add(item, () =>
                {
                    foreach (var subAction in action.DeferredSubActionsEnumerator())
                        submenu.AddItem(CreateMenuItem(sender, subAction, arguments));

                    Console.WriteLine($"Added {submenu.Items.Length} deferred sub action(s).");
                });

                item.Submenu = submenu;
            }

            return item;
        }

        public ZTableView TableView { get; }

        public RepositoryTableDataSource DataSource { get; }

        public IRepositoryActionProvider RepositoryActionProvider { get; }

        public IHumanizer Humanizer { get; }
    }
}