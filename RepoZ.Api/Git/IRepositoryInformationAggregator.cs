namespace RepoZ.Api.Git
{
	public interface IRepositoryInformationAggregator
	{
		string Get(string path);
	}
}