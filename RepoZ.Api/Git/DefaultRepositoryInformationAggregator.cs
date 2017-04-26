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
		private ObservableCollection<Repository> _dataSource = new ObservableCollection<Repository>();
		private StatusCompressor _compressor;

		public DefaultRepositoryInformationAggregator(StatusCompressor compressor)
		{
			_compressor = compressor;
		}

		public void Add(Repository repository)
		{
			_dataSource.Remove(repository);
			_dataSource.Add(repository);
		}

		public void RemoveByPath(string path)
		{
			var reposToRemove = _dataSource.Where(r => r.Path.Equals(path, StringComparison.OrdinalIgnoreCase)).ToArray();

			for (int i = reposToRemove.Length - 1; i >= 0; i--)
				_dataSource.Remove(reposToRemove[i]);
		}

		public string GetStatusByPath(string path)
		{
			if (!_dataSource.Any())
				return string.Empty;

			if (!path.EndsWith("\\", StringComparison.Ordinal))
				path += "\\";

			var repos = _dataSource.Where(r => path.StartsWith(r.Path, StringComparison.OrdinalIgnoreCase));

			if (!repos.Any())
				return string.Empty;

			var repo = repos.OrderByDescending(r => r.Path.Length).First();

			string status = _compressor.Compress(repo);

			if (string.IsNullOrEmpty(status))
				return repo.CurrentBranch;

			return repo.CurrentBranch + " " + status;
		}

		public IEnumerable<Repository> Repositories => _dataSource;
	}
}
