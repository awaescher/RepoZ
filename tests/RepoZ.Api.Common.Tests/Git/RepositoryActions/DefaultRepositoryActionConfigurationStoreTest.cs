namespace RepoZ.Api.Common.Tests.Git.RepositoryActions;

using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using EasyTestFileXunit;
using FakeItEasy;
using FluentAssertions;
using RepoZ.Api.Common.Git;
using RepoZ.Api.IO;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class DefaultRepositoryActionConfigurationStoreTest
{
    private readonly IErrorHandler _errorHandler = A.Fake<IErrorHandler>();
    private readonly IAppDataPathProvider _appDataPathProvider = A.Fake<IAppDataPathProvider>();
    private readonly IFileSystem _fileSystem = new FileSystem();

    public DefaultRepositoryActionConfigurationStoreTest()
    {
        A.CallTo(() => _appDataPathProvider.GetAppDataPath()).Returns("GetAppDataPath");
        A.CallTo(() => _appDataPathProvider.GetAppResourcesPath()).Returns("GetAppResourcesPath");
    }

    [Fact]
    public async Task LoadRepositoryActionConfiguration_ShouldReturnExpectedResult_WhenInputIsValid()
    {
        // arrange
        var sut = new DefaultRepositoryActionConfigurationStore(_errorHandler, _appDataPathProvider, _fileSystem);
        await using Stream stream = await EasyTestFile.LoadAsStream();

        // act
        RepositoryActionConfiguration configuration = await sut.LoadRepositoryActionConfiguration(stream);

        // assert
        A.CallTo(_errorHandler).MustNotHaveHappened();
        configuration.State.Should().Be(RepositoryActionConfiguration.LoadState.Ok);
        configuration.LoadError.Should().BeNullOrWhiteSpace();
        await Verifier.Verify(configuration);
    }
}