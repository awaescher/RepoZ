namespace RepoZ.Api.Git
{
    public interface IRepositoryTagsResolver
    {
        void UpdateTags(Repository repository);
    }
}