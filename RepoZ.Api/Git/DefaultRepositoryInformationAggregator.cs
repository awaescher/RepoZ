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
			if (!_dataSource.Any())
				return string.Empty;

			if (!path.EndsWith("\\", StringComparison.Ordinal))
				path += "\\";

			var views = _dataSource.ToList() // threading issues :(
				.Where(r => path.StartsWith(r.Path, StringComparison.OrdinalIgnoreCase));

			if (!views.Any())
				return string.Empty;

			var view = views.OrderByDescending(r => r.Path.Length).First();

			string status = _compressor.Compress(view.Repository);

			if (string.IsNullOrEmpty(status))
				return view.CurrentBranch;

			return view.CurrentBranch + " " + status;
		}

		public ObservableCollection<RepositoryView> Repositories => _dataSource;
	}
}
