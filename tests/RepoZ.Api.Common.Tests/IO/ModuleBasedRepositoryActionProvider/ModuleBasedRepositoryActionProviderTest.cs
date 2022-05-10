namespace RepoZ.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using System.Reflection.Metadata;
using FluentAssertions;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoZ.Api.Git;
using Xunit;

public class ModuleBasedRepositoryActionProviderTest
{
    private readonly Api.Git.Repository _repo;

    public ModuleBasedRepositoryActionProviderTest()
    {
        _repo = new Api.Git.Repository();
    }

    [Fact]
    public void GetPrimaryAction_ShouldReturnNull_WhenNoActionsProvided()
    {
        // arrange
        var sut = new ModuleBasedRepositoryActionProvider();

        // act
        RepositoryAction result = sut.GetPrimaryAction(_repo);

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetSecondaryAction_ShouldReturnNull_WhenNoActionsProvided()
    {
        // arrange
        var sut = new ModuleBasedRepositoryActionProvider();

        // act
        RepositoryAction result = sut.GetSecondaryAction(_repo);

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetContextMenuActions_ShouldReturnEmpty_WhenNoActionsProvided()
    {
        // arrange
        var sut = new ModuleBasedRepositoryActionProvider();

        // act
        IEnumerable<RepositoryAction> result = sut.GetContextMenuActions(new List<Repository>() { _repo, });

        // assert
        result.Should().BeEmpty();
    }
}