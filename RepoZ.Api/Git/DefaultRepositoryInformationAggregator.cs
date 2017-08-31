using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public class DefaultRepositoryInformationAggregator : IRepositoryInformationAggregator
	{
		private ObservableCollection<RepositoryView> _dataSource = new ObservableCollection<RepositoryView>();
		private StatusCompressor _compressor;

		public DefaultRepositoryInformationAggregator(StatusCompressor compressor)
		{
			_compressor = compressor;
		}

		public void Add(Repository repository)
		{
			var view = new RepositoryView(repository);
			_dataSource.Remove(view);
			_dataSource.Add(view);
		}

		public void RemoveByPath(string path)
		{
			var viewsToRemove = _dataSource.Where(r => r.Path.Equals(path, StringComparison.OrdinalIgnoreCase)).ToArray();

			for (int i = viewsToRemove.Length - 1; i >= 0; i--)
				_dataSource.Remove(viewsToRemove[i]);
		}

		public string GetStatusByPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return string.Empty;

			List<RepositoryView> views = null;
			try
			{
				views = _dataSource?.ToList();
			}
			catch (ArgumentException)
			{ /* there are extremely rare threading issues with System.Array.Copy() here */ }

			var hasAny = views?.Any() ?? false;
			if (!hasAny)
				return string.Empty;

			if (!path.EndsWith("\\", StringComparison.Ordinal))
				path += "\\";

			var viewsByPath = views.Where(r => r?.Path != null && path.StartsWith(r.Path, StringComparison.OrdinalIgnoreCase));

			if (!viewsByPath.Any())
				return string.Empty;

			var view = viewsByPath.OrderByDescending(r => r.Path.Length).First();

			string status = _compressor.Compress(view.Repository);

			if (string.IsNullOrEmpty(status))
				return view.CurrentBranch;

			return view.CurrentBranch + " " + status;
		}

		public ObservableCollection<RepositoryView> Repositories => _dataSource;
	}
}
