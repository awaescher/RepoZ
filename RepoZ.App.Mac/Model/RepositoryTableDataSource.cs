using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AppKit;
using Foundation;
using RepoZ.Api.Git;

namespace RepoZ.App.Mac.Model
{
    public class RepositoryTableDataSource : NSTableViewDataSource
    {
        public EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;

        private RepositoryView[] _sortedRepositories;
        private ObservableCollection<RepositoryView> _repositories;

        public RepositoryTableDataSource(ObservableCollection<RepositoryView> repositories)
        {
            _repositories = repositories;
            SortAndFilterInternally();

            _repositories.CollectionChanged += Repositories_CollectionChanged;
        }

        protected override void Dispose(bool disposing)
        {
            _repositories.CollectionChanged -= Repositories_CollectionChanged;

            base.Dispose(disposing);
        }

        void Repositories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SortAndFilterInternally();
            CollectionChanged?.Invoke(sender, e);
        }

        public void Filter(string value)
        {
            if (string.Equals(CurrentFilter, value))
                return;

            CurrentFilter = value;

            SortAndFilterInternally();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void SortAndFilterInternally()
        {
            Func<RepositoryView, bool> filterFunc = r => true;

            if (!string.IsNullOrEmpty(CurrentFilter))
                filterFunc = r => r.MatchesFilter(CurrentFilter);

            _sortedRepositories = _repositories
                .OrderBy(r => r.Name)
                .Where(filterFunc)
                .ToArray();
        }

        public RepositoryView GetRepositoryViewByIndex(int index)
        {
            if (index < 0 || _sortedRepositories?.Length <= index)
                return null;

            return _sortedRepositories[index];
        }

        public override nint GetRowCount(NSTableView tableView)
        {
            return _sortedRepositories?.Length ?? 0;
        }

        public string CurrentFilter { get; private set; }
    }
}
