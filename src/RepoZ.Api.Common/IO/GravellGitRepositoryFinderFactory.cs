namespace RepoZ.Api.Common.IO;

using System;
using RepoZ.Api.IO;

public class GravellGitRepositoryFinderFactory : ISingleGitRepositoryFinderFactory
{
    private const string FACTORY_NAME = "GravellGitRepositoryFinder";

    private readonly IPathSkipper _pathSkipper;

    public GravellGitRepositoryFinderFactory(IPathSkipper pathSkipper)
    {
        _pathSkipper = pathSkipper ?? throw new ArgumentNullException(nameof(pathSkipper));
    }

    public string Name => FACTORY_NAME;

    public bool IsActive { get; } = true;

    public IGitRepositoryFinder Create()
    {
        return new GravellGitRepositoryFinder(_pathSkipper);
    }
}