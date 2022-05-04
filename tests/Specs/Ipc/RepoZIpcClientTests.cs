namespace Specs.Ipc
{
    using NUnit.Framework;
    using Moq;
    using RepoZ.Ipc;
    using FluentAssertions;
    using RepoZ.Plugin.IpcService;

    public class RepoZIpcClientTests
    {
        private Mock<IRepositorySource> _repositorySource;
        private IpcClient _client;
        private IpcServer _server;

        [SetUp]
        public void Setup()
        {
            var endpoint = new TestIpcEndpoint();
            _repositorySource = new Mock<IRepositorySource>();

            _client = new IpcClient(endpoint);
            _server = new IpcServer(endpoint, _repositorySource.Object);
        }

        public class GetRepositoriesMethod : RepoZIpcClientTests
        {
            [Test]
            public void Returns_An_Error_Message_If_RepoZ_Is_Not_Reachable()
            {
                _server.Stop();

                IpcClient.Result result = _client.GetRepositories();
                result.Answer.Should().StartWith("RepoZ seems"); // ... to be running but ... -> indicates an error
            }

            [Test]
            public void Returns_Deserialized_Matching_Repositories()
            {
                _server.Start();
                _repositorySource
                    .Setup(rs => rs.GetMatchingRepositories(It.IsAny<string>()))
                    .Returns(new Repository[]
                        {
                            new Repository()
                                {
                                    Name = "N",
                                    BranchWithStatus = "B",
                                    Path = "P",
                                },
                        });

                IpcClient.Result result = _client.GetRepositories();

                _server.Stop();

                result.Repositories.Should().HaveCount(1);
            }
        }
    }
}