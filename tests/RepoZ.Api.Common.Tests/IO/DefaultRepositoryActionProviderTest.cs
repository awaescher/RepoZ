namespace RepoZ.Api.Common.Tests.IO;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
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
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Common.IO;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
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
    private readonly List<IVariableProvider> _providers;
    private readonly List<IMethod> _methods;

    public DefaultRepositoryActionProviderTest()
    {
        A.CallTo(() => _appDataPathProvider.GetAppDataPath()).Returns("GetAppDataPath");
        A.CallTo(() => _appDataPathProvider.GetAppResourcesPath()).Returns("GetAppResourcesPath");

        A.CallTo(() => _translationService.Translate(A<string>._)).ReturnsLazily(call => call.Arguments[0] as string);

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

        _providers = new List<IVariableProvider>
            {
                new DateTimeNowVariableProvider(dateTimeNowVariableProviderOptions),
                new DateTimeTimeVariableProvider(dateTimeTimeVariableProviderOptions),
                new DateTimeDateVariableProvider(dateTimeDateVariableProviderOptions),
                new EmptyVariableProvider(),
                new CustomEnvironmentVariableVariableProvider(_fileSystem),
                new RepositoryVariableProvider(),
                new SlashVariableProvider(),
                new BackslashVariableProvider(),
            };

        _methods = new List<IMethod>
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
        var sut = new DefaultRepositoryActionProvider(_repositoryActionConfigurationStore, _repositoryWriter, _repositoryMonitor, _errorHandler, _translationService, _fileSystem, new RepositoryExpressionEvaluator(_providers, _methods));
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
                result.Name,
                result.CanExecute,
                result.ExecutionCausesSynchronizing,
            }).ModifySerialization(s => s.DontIgnoreFalse());
    }
}