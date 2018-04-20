using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AppKit;
using RepoZ.Api.Git;

namespace RepoZ.UI.Mac.Story.Model
{
    public class RepositoryTableDataSource : NSTableViewDataSource
    {
        public ObservableCollection<RepositoryView> Repositories;

        public RepositoryTableDataSource(ObservableCollection<RepositoryView> repositories)
        {
            Repositories = repositories;
        }

        public override nint GetRowCount(NSTableView tableView)
        {
            return Repositories?.Count ?? 0;
        }
    }
}
