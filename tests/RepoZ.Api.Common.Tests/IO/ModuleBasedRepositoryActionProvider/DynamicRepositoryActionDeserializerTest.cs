namespace RepoZ.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider;

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using VerifyTests;
using VerifyXunit;
using Xunit;

public static class VerifierInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.DisableRequireUniquePrefix();
    }
}

[UsesEasyTestFile]
[UsesVerify]
public class DynamicRepositoryActionDeserializerTest
{
    private readonly DynamicRepositoryActionDeserializer _sut;

    public DynamicRepositoryActionDeserializerTest()
    {
        _sut = new DynamicRepositoryActionDeserializer();
    }

    [Fact]
    public async Task Deserialize_ShouldReturnEmptyObject_WhenContentIsEmptyJson()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("EmptyJson");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithVariables_WhenContentIsVariablesOnly()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("VariablesOnly1");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags1()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("RepositoryTags1");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithLatestTags_WhenContentHasDoubleTags()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("RepositoryTagsDouble");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags2()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("RepositoryTags2");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result).UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags1));
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags3()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("RepositoryTags3");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Deserialize_Sample1()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("Sample1");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }
}