namespace RepoZ.Api.Common.Git;

using System.Linq;
using LibGit2Sharp;
using RepoZ.Api.Git;
using System.IO;
using System;
using Repository = RepoZ.Api.Git.Repository;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

public class DefaultRepositoryReader : IRepositoryReader
{
    private readonly IRepositoryTagsFactory _resolver;

    public DefaultRepositoryReader(IRepositoryTagsFactory resolver)
    {
        _resolver = resolver;
    }

    public Api.Git.Repository ReadRepository(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return Api.Git.Repository.Empty;
        }

        var repoPath = LibGit2Sharp.Repository.Discover(path);
        if (string.IsNullOrEmpty(repoPath))
        {
            return Api.Git.Repository.Empty;
        }

        Repository result = ReadRepositoryWithRetries(repoPath, 3);
        if (result != null)
        {
            result.Tags = _resolver.GetTags(result).ToArray();
        }
        return result;
    }

    private Api.Git.Repository ReadRepositoryWithRetries(string repoPath, int maxRetries)
    {
        Api.Git.Repository repository = null;
        var currentTry = 1;

        while (repository == null && currentTry <= maxRetries)
        {
            try
            {
                repository = ReadRepositoryInternal(repoPath);
            }
            catch (LockedFileException)
            {
                if (currentTry >= maxRetries)
                {
                    throw;
                }
                else
                {
                    System.Threading.Thread.Sleep(500);
                }
            }

            currentTry++;
        }

        return repository;
    }

    private Api.Git.Repository ReadRepositoryInternal(string repoPath)
    {
        try
        {
            using var repo = new LibGit2Sharp.Repository(repoPath);
            RepositoryStatus status = repo.RetrieveStatus();

            var workingDirectory = new DirectoryInfo(repo.Info.WorkingDirectory);

            HeadDetails headDetails = GetHeadDetails(repo);

            return new Api.Git.Repository()
                {
                    Name = workingDirectory.Name,
                    Path = workingDirectory.FullName,
                    Location = workingDirectory.Parent.FullName,
                    Branches = repo.Branches.Select(b => b.FriendlyName).ToArray(),
                    LocalBranches = repo.Branches.Where(b => !b.IsRemote).Select(b => b.FriendlyName).ToArray(),
                    AllBranchesReader = () => ReadAllBranches(repoPath),
                    CurrentBranch = headDetails.Name,
                    CurrentBranchHasUpstream = !string.IsNullOrEmpty(repo.Head.UpstreamBranchCanonicalName),
                    CurrentBranchIsDetached = headDetails.IsDetached,
                    CurrentBranchIsOnTag = headDetails.IsOnTag,
                    AheadBy = repo.Head.TrackingDetails?.AheadBy,
                    BehindBy = repo.Head.TrackingDetails?.BehindBy,
                    LocalUntracked = status?.Untracked.Count(),
                    LocalModified = status?.Modified.Count(),
                    LocalMissing = status?.Missing.Count(),
                    LocalAdded = status?.Added.Count(),
                    LocalStaged = status?.Staged.Count(),
                    LocalRemoved = status?.Removed.Count(),
                    LocalIgnored = status?.Ignored.Count(),
                    RemoteUrls = repo.Network?.Remotes?.Select(r => r.Url).Where(url => !string.IsNullOrEmpty(url)).ToArray() ?? Array.Empty<string>(),
                    StashCount = repo.Stashes?.Count() ?? 0,
                    Tags = Array.Empty<string>(),
                };
        }
        catch (Exception)
        {
            return Api.Git.Repository.Empty;
        }
    }

    private static string[] ReadAllBranches(string repoPath)
    {
        try
        {
            using var repo = new LibGit2Sharp.Repository(repoPath);
            var localBranches = repo.Branches.Where(b => !b.IsRemote).Select(b => b.FriendlyName).ToList();

            // "origin/" is removed from remote branches name and HEAD branch is ignored
            var strippedRemoteBranches = repo.Branches
                                             .Where(b => b.IsRemote && b.FriendlyName.IndexOf("HEAD") == -1)
                                             .Select(b => b.FriendlyName.Replace("origin/", ""))
                                             .ToList();

            return strippedRemoteBranches
                   .Except(localBranches)
                   .OrderBy(n => n)
                   .ToArray();
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }

    private HeadDetails GetHeadDetails(LibGit2Sharp.Repository repo)
    {
        // unfortunately, type DetachedHead is internal ...
        var isDetached = repo.Head.GetType().Name.EndsWith("DetachedHead", StringComparison.OrdinalIgnoreCase);

        Tag tag = null;

        var headTipSha = repo.Head.Tip?.Sha;
        if (isDetached && headTipSha != null)
        {
            tag = repo.Tags.FirstOrDefault(t => t.Target?.Sha?.Equals(repo.Head.Tip.Sha) ?? false);
        }

        return new HeadDetails()
            {
                Name = isDetached
                    ? tag?.FriendlyName ?? headTipSha ?? repo.Head.FriendlyName
                    : repo.Head.FriendlyName,
                IsDetached = isDetached,
                IsOnTag = tag != null,
            };
    }

    internal class HeadDetails
    {
        internal string Name { get; set; }
        internal bool IsDetached { get; set; }
        internal bool IsOnTag { get; set; }
    }
}