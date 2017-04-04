namespace RepoZ.Api.Git
{
	public interface IRepositoryReader
	{
		Repository ReadRepository(string path);
	}
}