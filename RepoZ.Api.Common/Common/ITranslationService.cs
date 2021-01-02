namespace RepoZ.Api.Common.Common
{
	public interface ITranslationService
	{
		string Translate(string value);
		string Translate(string value, params object[] args);
	}
}
