using System.Collections.ObjectModel;
using RepoZ.Api.Git;

namespace Specs.Mocks
{
	internal class UselessRepositoryInformationAggregator : IRepositoryInformationAggregator
	{
		private ObservableCollection<RepositoryView> _repositories = new ObservableCollection<RepositoryView>();

		public void Add(Repository repository)
		{
		}

		public string GetStatusByPath(string path) => "n/a";

		public bool HasRepository(string path) => false;

		public void RemoveByPath(string path)
		{
		}

		public ObservableCollection<RepositoryView> Repositories => _repositories;

	}
}