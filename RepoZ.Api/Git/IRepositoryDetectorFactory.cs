namespace RepoZ.Api.Git
{
    public interface IRepositoryDetectorFactory
    {
        IRepositoryDetector Create();
    }
}