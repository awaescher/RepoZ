namespace branch.Shared
{
	public interface IRepositoryHelper
	{
		RepositoryHelper.RepositoryInfo ReadRepository(string path);
	}
}