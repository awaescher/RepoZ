namespace RepoZ.Api.Common.Git
{
    using RepoZ.Api.Git;

    public class DefaultRepositoryDetectorFactory : IRepositoryDetectorFactory
    {
        private readonly IRepositoryReader _repositoryReader;

        public DefaultRepositoryDetectorFactory(IRepositoryReader repositoryReader)
        {
            _repositoryReader = repositoryReader;
        }

        public IRepositoryDetector Create()
        {
            return new DefaultRepositoryDetector(_repositoryReader);
        }
    }
}