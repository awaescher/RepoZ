using RepoZ.Shared.Git;

namespace RepoZ.Shared
{
	public interface IRepositoryReader
	{
		RepositoryInfo ReadRepository(string path);
	}
}