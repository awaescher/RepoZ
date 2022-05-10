namespace RepoZ.Api.Common.IO;

using System;
using System.IO.Abstractions;
using JetBrains.Annotations;
using RepoZ.Api.IO;

public class GravellGitRepositoryFinderFactory : ISingleGitRepositoryFinderFactory
{
    private const string FACTORY_NAME = "GravellGitRepositoryFinder";

    private readonly IPathSkipper _pathSkipper;
    private readonly IFileSystem _fileSystem;

    public GravellGitRepositoryFinderFactory(IPathSkipper pathSkipper, IFileSystem fileSystem)
    {
        _pathSkipper = pathSkipper ?? throw new ArgumentNullException(nameof(pathSkipper));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public string Name => FACTORY_NAME;

    public bool IsActive { get; } = true;

    public IGitRepositoryFinder Create()
    {
        return new GravellGitRepositoryFinder(_pathSkipper, _fileSystem);
    }
}