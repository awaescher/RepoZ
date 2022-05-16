namespace RepoZ.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FakeItEasy;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class RepositorySpecificConfigurationTest
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly MockFileSystem _fileSystem;
    private readonly DynamicRepositoryActionDeserializer _appsettingsDeserializer;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;
    private readonly string _tempPath;

    public RepositorySpecificConfigurationTest()
    {
        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("json");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");

        _tempPath = Path.GetTempPath();
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), "C:\\");

        _appDataPathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => _appDataPathProvider.GetAppDataPath()).Returns(_tempPath);

        _appsettingsDeserializer = new DynamicRepositoryActionDeserializer(
            new ActionDeserializerComposition(
                new ActionBrowserV1Deserializer(),
                new ActionFolderV1Deserializer(),
                new ActionSeparatorV1Deserializer()));
    }

    [Fact(Skip = "Because wip")]
    public async Task Wip()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActions1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _fileSystem.AddFile(Path.Combine(_tempPath, "appsettings.json"), new MockFileData(content, Encoding.UTF8));
        var sut = new RepositorySpecificConfiguration(_appDataPathProvider, _fileSystem, _appsettingsDeserializer);

        // act
        var result = sut.Create(new Repository());

        // assert
        await Verifier.Verify(result, _verifySettings);
    }
}