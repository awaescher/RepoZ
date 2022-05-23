namespace RepoZ.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider.Action;

using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FluentAssertions;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class ExecutableV1Test
{
    private readonly DynamicRepositoryActionDeserializer _sut;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;

    public ExecutableV1Test()
    {
        _sut = DynamicRepositoryActionDeserializerFactory.CreateWithDeserializer(new ActionExecutableV1Deserializer());

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("json");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
    }

    [Fact]
    public async Task Deserialize_ExecutableV1()
    {
        // arrange
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldBeOfExpectedType()
    {
        // arrange
        _testFileSettings.UseMethodName(nameof(Deserialize_ExecutableV1));
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        _ = result.ActionsCollection.Actions.Should().AllBeOfType<RepositoryActionExecutableV1>();
    }
}