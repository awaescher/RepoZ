namespace RepoZ.Api.Git
{
	public interface IRepositoryWriter
	{
		bool Checkout(Repository repository, string branchName);
	}
}