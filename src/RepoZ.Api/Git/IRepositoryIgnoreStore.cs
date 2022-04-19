namespace RepoZ.Api.Git
{
    public interface IRepositoryIgnoreStore
    {
        void IgnoreByPath(string path);

        bool IsIgnored(string path);

        void Reset();
    }
}