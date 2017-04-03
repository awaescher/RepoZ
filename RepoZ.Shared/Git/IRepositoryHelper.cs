namespace RepoZ.Shared
{
	public interface IRepositoryReader
	{
		RepositoryReader.RepositoryInfo ReadRepository(string path);
	}
}