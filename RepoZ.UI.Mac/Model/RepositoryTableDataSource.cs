using System;
using System.Collections.Generic;
using AppKit;
using RepoZ.Api.Git;

namespace RepoZ.UI.Mac.Model
{
	public class RepositoryTableDataSource : NSTableViewDataSource
	{
		public List<RepositoryView> Repositories = new List<RepositoryView>();

		public RepositoryTableDataSource()
		{
		}

		public override nint GetRowCount(NSTableView tableView)
		{
			return Repositories.Count;
		}
	}
}
