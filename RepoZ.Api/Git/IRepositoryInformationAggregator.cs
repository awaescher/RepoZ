using System.Collections.ObjectModel;

namespace RepoZ.Api.Git
{
	public interface IRepositoryInformationAggregator
	{
		void Add(Repository repository);

		void RemoveByPath(string path);

		string GetStatusByPath(string path);

		ObservableCollection<RepositoryView> Repositories { get; }

		void Reset();

		bool HasRepository(string path);
	}
}