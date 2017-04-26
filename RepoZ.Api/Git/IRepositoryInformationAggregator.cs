using System.Collections.Generic;

namespace RepoZ.Api.Git
{
	public interface IRepositoryInformationAggregator
	{
		void Add(Repository repository);

		void RemoveByPath(string path);

		string GetStatusByPath(string path);

		IEnumerable<Repository> Repositories { get; }
	}
}