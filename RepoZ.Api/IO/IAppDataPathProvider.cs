namespace RepoZ.Api.IO
{
	public interface IAppDataPathProvider
	{
		string GetAppDataPath();

		string GetAppResourcesPath();
	}
}
