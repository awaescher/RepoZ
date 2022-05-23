namespace RepoZ.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FluentAssertions;
using Newtonsoft.Json;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class DynamicRepositoryActionDeserializerTest
{
    private readonly DynamicRepositoryActionDeserializer _sut;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;

    public DynamicRepositoryActionDeserializerTest()
    {
        _sut = DynamicRepositoryActionDeserializerFactory.Create();

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("json");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
    }

    [Fact]
    public async Task Deserialize_ShouldReturnEmptyObject_WhenContentIsEmptyJson()
    {
        // arrange
        _testFileSettings.UseFileName("EmptyJson");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithVariables_WhenContentIsVariablesOnly()
    {
        // arrange
        _testFileSettings.UseFileName("VariablesOnly1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags1()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTags1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithLatestTags_WhenContentHasDoubleTags()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTagsDouble");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags2()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTags2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings).UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags1));
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags3()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTags3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions1()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActions1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions2()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActions2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings).UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions1));
    }
    
    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions3()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActions3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect1()
    {
        // arrange
        _testFileSettings.UseFileName("Redirect1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect2()
    {
        // arrange
        _testFileSettings.UseFileName("Redirect2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings).UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect1));
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect3()
    {
        // arrange
        _testFileSettings.UseFileName("Redirect3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObject_WhenContentIsRepositorySpecificEnvFile1()
    {
        // arrange
        _testFileSettings.UseFileName("RepositorySpecificEnvFile1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObject_WhenContentIsRepositorySpecificConfigFile1()
    {
        // arrange
        _testFileSettings.UseFileName("RepositorySpecificConfigFile1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_Sample1()
    {
        // arrange
        _testFileSettings.UseFileName("Sample1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_Sample2()
    {
        // arrange
        _testFileSettings.UseFileName("Sample2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_Sample3()
    {
        // arrange
        _testFileSettings.UseFileName("Sample3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public void EmptyFile_ShouldThrow()
    {
        // arrange

        // act
        Func<RepositoryActionConfiguration> act = () => _ = _sut.Deserialize(string.Empty);

        // assert
        _ = act.Should().Throw<JsonReaderException>().WithMessage("Error reading JObject from JsonReader. Path '', line 0, position 0.");
    }
}