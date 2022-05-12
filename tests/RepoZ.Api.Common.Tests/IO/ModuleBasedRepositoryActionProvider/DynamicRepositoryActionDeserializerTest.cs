namespace RepoZ.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider;

using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using VerifyXunit;
using Xunit;

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
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions1()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("RepositoryActions1");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions2()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("RepositoryActions2");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result).UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions1));
    }
    
    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions3()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("RepositoryActions3");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect1()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("Redirect1");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect2()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("Redirect2");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result).UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect1));
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect3()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("Redirect3");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObject_WhenContentIsRepositorySpecificEnvFile1()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("RepositorySpecificEnvFile1");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObject_WhenContentIsRepositorySpecificConfigFile1()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("RepositorySpecificConfigFile1");
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

    [Fact]
    public async Task Deserialize_Sample2()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("Sample2");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Deserialize_Sample3()
    {
        // arrange
        var settings = new EasyTestFileSettings();
        settings.UseFileName("Sample3");
        settings.UseExtension("json");

        var content = await EasyTestFile.LoadAsText(settings);

        // act
        var result = await _sut.DeserializeAsync(content);

        // assert
        await Verifier.Verify(result);
    }
}