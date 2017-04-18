namespace RepoZ.Api.Git
{
	public interface IRepositoryInformationAggregator
	{
		string Get(string path);

		string GetFormatted(string path, string upSign, string downSign);
	}
}