namespace RepoZ.Api.Common.Common
{
	public interface IAppSettingsProvider
	{
		AppSettings Load();

		void Save(AppSettings settings);
	}
}
