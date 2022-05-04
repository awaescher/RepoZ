namespace RepoZ.Api.Common.IO;

using System;
using RepoZ.Api.IO;

public class GravellGitRepositoryFinderFactory : ISingleGitRepositoryFinderFactory
{
    private readonly IPathSkipper _pathSkipper;

    public GravellGitRepositoryFinderFactory(IPathSkipper pathSkipper)
    {
        _pathSkipper = pathSkipper ?? throw new ArgumentNullException(nameof(pathSkipper));
    }

    public string Name { get; } = "GravellGitRepositoryFinder";

    public bool IsActive { get; } = true;

    public IGitRepositoryFinder Create()
    {
        return new GravellGitRepositoryFinder(_pathSkipper);
    }
}