namespace RepoZ.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using ExpressionStringEvaluator.Methods.BooleanToBoolean;
using ExpressionStringEvaluator.Methods.Flow;
using ExpressionStringEvaluator.Methods.StringToBoolean;
using ExpressionStringEvaluator.Methods.StringToInt;
using ExpressionStringEvaluator.Methods.StringToString;
using ExpressionStringEvaluator.Methods;
using ExpressionStringEvaluator.VariableProviders.DateTime;
using ExpressionStringEvaluator.VariableProviders;
using FakeItEasy;
using FluentAssertions;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
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
    private readonly RepositoryExpressionEvaluator _repositoryExpressionEvaluator;
    private readonly ActionMapperComposition _actionMapperComposition;
    private readonly ITranslationService _translationService;
    private readonly IErrorHandler _errorHandler;

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

        _appsettingsDeserializer = DynamicRepositoryActionDeserializerFactory.Create();

        var dateTimeTimeVariableProviderOptions = new DateTimeVariableProviderOptions()
        {
            DateTimeProvider = () => DateTime.Now,
        };

        var dateTimeNowVariableProviderOptions = new DateTimeNowVariableProviderOptions()
        {
            DateTimeProvider = () => DateTime.Now,
        };

        var dateTimeDateVariableProviderOptions = new DateTimeDateVariableProviderOptions()
        {
            DateTimeProvider = () => DateTime.Now,
        };

        var providers = new List<IVariableProvider>
            {
                new DateTimeNowVariableProvider(dateTimeNowVariableProviderOptions),
                new DateTimeTimeVariableProvider(dateTimeTimeVariableProviderOptions),
                new DateTimeDateVariableProvider(dateTimeDateVariableProviderOptions),
                new EmptyVariableProvider(),
                new CustomEnvironmentVariableVariableProvider(),
                new RepositoryVariableProvider(),
                new SlashVariableProvider(),
                new BackslashVariableProvider(),
            };

        var methods = new List<IMethod>
            {
                new StringTrimEndStringMethod(),
                new StringTrimStartStringMethod(),
                new StringTrimStringMethod(),
                new StringContainsStringMethod(),
                new StringLowerStringMethod(),
                new StringUpperStringMethod(),
                new UrlEncodeStringMethod(),
                new UrlDecodeStringMethod(),
                new StringEqualsStringMethod(),
                new AndBooleanMethod(),
                new OrBooleanMethod(),
                new StringIsNullOrEmptyBooleanMethod(),
                new FileExistsBooleanMethod(),
                new NotBooleanMethod(),
                new StringLengthMethod(),
                new IfThenElseMethod(),
                new IfThenMethod(),
                new InMethod(),
                new StringReplaceMethod(),
                new SubstringMethod(),
            };

        _repositoryExpressionEvaluator = new RepositoryExpressionEvaluator(providers, methods);

        _translationService = A.Fake<ITranslationService>();
        A.CallTo(() => _translationService.Translate(A<string>._)).ReturnsLazily(call => call.Arguments[0] as string);
        _errorHandler = A.Fake<IErrorHandler>();
        IRepositoryWriter repositoryWriter = A.Fake<IRepositoryWriter>();
        IRepositoryMonitor repositoryMonitor = A.Fake<IRepositoryMonitor>();

        _actionMapperComposition = ActionMapperCompositionFactory.Create(
            _repositoryExpressionEvaluator,
            _translationService,
            _errorHandler,
            _fileSystem,
            repositoryWriter,
            repositoryMonitor);
    }

    [Fact]
    public async Task Create_ShouldRespectMultiSelectRepos()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActionsMultiSelect");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _fileSystem.AddFile(Path.Combine(_tempPath, RepositoryConfigurationReader.FILENAME), new MockFileData(content, Encoding.UTF8));
        var sut = new RepositorySpecificConfiguration(
            _fileSystem,
            _repositoryExpressionEvaluator,
            _actionMapperComposition,
            _translationService,
            _errorHandler,
            new RepositoryConfigurationReader(
                _appDataPathProvider,
                _fileSystem,
                _appsettingsDeserializer,
                _repositoryExpressionEvaluator));

        // act
        IEnumerable<RepositoryAction> result = sut.CreateActions(new Repository(), new Repository());

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Create_ShouldNotCareAboutMultiSelectRepos_WhenSingleRepo()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActionsMultiSelect");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _fileSystem.AddFile(Path.Combine(_tempPath, RepositoryConfigurationReader.FILENAME), new MockFileData(content, Encoding.UTF8));
        var sut = new RepositorySpecificConfiguration(
            _fileSystem,
            _repositoryExpressionEvaluator,
            _actionMapperComposition,
            _translationService,
            _errorHandler,
            new RepositoryConfigurationReader(
                _appDataPathProvider,
                _fileSystem,
                _appsettingsDeserializer,
                _repositoryExpressionEvaluator));

        // act
        IEnumerable<RepositoryAction> result = sut.CreateActions(new Repository());

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Create_ShouldOnlyProcessActiveItems()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActions1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _fileSystem.AddFile(Path.Combine(_tempPath, RepositoryConfigurationReader.FILENAME), new MockFileData(content, Encoding.UTF8));
        var sut = new RepositorySpecificConfiguration(
            _fileSystem,
            _repositoryExpressionEvaluator,
            _actionMapperComposition,
            _translationService,
            _errorHandler,
            new RepositoryConfigurationReader(
                _appDataPathProvider,
                _fileSystem,
                _appsettingsDeserializer,
                _repositoryExpressionEvaluator));

        // act
        IEnumerable<RepositoryAction> result = sut.CreateActions(new Repository());

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_ShouldProcessSeparator1()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActionsWithSeparator1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _fileSystem.AddFile(Path.Combine(_tempPath, RepositoryConfigurationReader.FILENAME), new MockFileData(content, Encoding.UTF8));
        var sut = new RepositorySpecificConfiguration(
            _fileSystem,
            _repositoryExpressionEvaluator,
            _actionMapperComposition,
            _translationService,
            _errorHandler,
            new RepositoryConfigurationReader(
                _appDataPathProvider,
                _fileSystem,
                _appsettingsDeserializer,
                _repositoryExpressionEvaluator));

        // act
        IEnumerable<RepositoryAction> result = sut.CreateActions(new Repository());

        // assert
        await Verifier.Verify(result, _verifySettings);
    }
}