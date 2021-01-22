namespace RepoZ.Api.Git
{
	public interface IRepositoryWriter
	{
		bool Checkout(Repository repository, string branchName);

		void Fetch(Repository repository);

		void Pull(Repository repository);

		void Push(Repository repository);
	}
}