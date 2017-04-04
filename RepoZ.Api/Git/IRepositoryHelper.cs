namespace RepoZ.Api.Git
{
	public interface IRepositoryReader
	{
		RepositoryInfo ReadRepository(string path);
	}
}