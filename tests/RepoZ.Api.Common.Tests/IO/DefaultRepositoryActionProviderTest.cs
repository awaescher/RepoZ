namespace RepoZ.Api.Common.Tests.IO;

using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using EasyTestFileXunit;
using FakeItEasy;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Common.IO;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class DefaultRepositoryActionProviderTest
{
    private readonly IErrorHandler _errorHandler = A.Fake<IErrorHandler>();
    private readonly IAppDataPathProvider _appDataPathProvider = A.Fake<IAppDataPathProvider>();
    private readonly IRepositoryActionConfigurationStore _repositoryActionConfigurationStore = A.Fake<IRepositoryActionConfigurationStore>();
    private readonly IRepositoryWriter _repositoryWriter = A.Fake<IRepositoryWriter>();
    private readonly IRepositoryMonitor _repositoryMonitor = A.Fake<IRepositoryMonitor>();
    private readonly ITranslationService _translationService = A.Fake<ITranslationService>();
    private readonly FileSystem _fileSystem = new FileSystem();

    public DefaultRepositoryActionProviderTest()
    {
        A.CallTo(() => _appDataPathProvider.GetAppDataPath()).Returns("GetAppDataPath");
        A.CallTo(() => _appDataPathProvider.GetAppResourcesPath()).Returns("GetAppResourcesPath");

        A.CallTo(() => _translationService.Translate(A<string>._)).ReturnsLazily(call => call.Arguments[0] as string);
    }

    [Fact]
    public async Task GetPrimaryAction_ShouldReturnFirstActiveAction_WhenConfigIsValid()
    {
        // arrange
        A.CallTo(() => _repositoryActionConfigurationStore.RepositoryActionConfiguration).Returns(new RepositoryActionConfiguration()
            {
                State = RepositoryActionConfiguration.LoadState.Ok,
                RepositoryActions = new List<RepositoryActionConfiguration.RepositoryAction>()
                    {
                        new RepositoryActionConfiguration.RepositoryAction()
                            {
                                Active = "false",
                                Name = "1-not-active",
                            },
                        new RepositoryActionConfiguration.RepositoryAction()
                            {
                                Active = "true",
                                Name = "2-active",
                                Command = "browser",
                                Arguments = "https://github.com/coenm/",
                            },
                        new RepositoryActionConfiguration.RepositoryAction()
                            {
                                Active = "true",
                                Name = "3-active",
                            },
                        new RepositoryActionConfiguration.RepositoryAction(),
                    },
            });
        var sut = new DefaultRepositoryActionProvider(_repositoryActionConfigurationStore, _repositoryWriter, _repositoryMonitor, _errorHandler, _translationService, _fileSystem);
        // await using Stream stream = await EasyTestFile.LoadAsStream();
        var repository = new Repository()
            {
                Path = "C:\\",
                Branches = new []{ "develop", "main", },
                LocalBranches = new[] { "develop", },
                RemoteUrls = new []{ "https://github.com/coenm/RepoZ.git", },
        };
    
        // act
        RepositoryAction result = sut.GetPrimaryAction(repository);
        
        // assert
        A.CallTo(_errorHandler).MustNotHaveHappened();
        await Verifier.Verify(new
            {
                result.BeginGroup,
                result.Name,
                result.CanExecute,
                result.ExecutionCausesSynchronizing,
            }).ModifySerialization(s => s.DontIgnoreFalse());
    }
}