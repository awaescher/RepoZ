using RepoZ.Api.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RepoZ.Api.Git
{
	public class DefaultRepositoryInformationAggregator : IRepositoryInformationAggregator
	{
		private readonly ObservableCollection<RepositoryView> _dataSource = new ObservableCollection<RepositoryView>();
		private readonly IThreadDispatcher _dispatcher;

		public DefaultRepositoryInformationAggregator(StatusCompressor compressor, IThreadDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public void Add(Repository repository)
		{
			_dispatcher.Invoke(() =>
			{
				var view = new RepositoryView(repository);

				_dataSource.Remove(view);
				_dataSource.Add(view);
			});
		}

		public void RemoveByPath(string path)
		{
			_dispatcher.Invoke(() =>
			{
				var viewsToRemove = _dataSource.Where(r => r.Path.Equals(path, StringComparison.OrdinalIgnoreCase)).ToArray();

				for (int i = viewsToRemove.Length - 1; i >= 0; i--)
					_dataSource.Remove(viewsToRemove[i]);
			});
		}

		public string GetStatusByPath(string path)
		{
			var view = GetRepositoryByPath(path);
			return view?.BranchWithStatus;
		}

		private RepositoryView GetRepositoryByPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;

			List<RepositoryView> views = null;
			try
			{
				views = _dataSource?.ToList();
			}
			catch (ArgumentException)
			{ /* there are extremely rare threading issues with System.Array.Copy() here */ }

			var hasAny = views?.Any() ?? false;
			if (!hasAny)
				return null;

			if (!path.EndsWith("\\", StringComparison.Ordinal))
				path += "\\";

			var viewsByPath = views.Where(r => r?.Path != null && path.StartsWith(r.Path, StringComparison.OrdinalIgnoreCase));

			if (!viewsByPath.Any())
				return null;

			return viewsByPath.OrderByDescending(r => r.Path.Length).First();
		}

		public bool HasRepository(string path)
		{
			return GetRepositoryByPath(path) != null;
		}

		public void Reset()
		{
			_dataSource.Clear();
		}

		public ObservableCollection<RepositoryView> Repositories => _dataSource;
	}
}
