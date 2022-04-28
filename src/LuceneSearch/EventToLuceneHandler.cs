namespace LuceneSearch;

using System;
using System.Linq;
using System.Threading.Tasks;
using RepoZ.Api;
using RepoZ.Api.Git;

internal class EventToLuceneHandler : IModule, IDisposable
{
    private readonly IRepositoryMonitor _monitor;
    private readonly IRepositoryIndex _index;
    private readonly IRepositorySearch _search;

    public EventToLuceneHandler(IRepositoryMonitor monitor, IRepositoryIndex index, IRepositorySearch search)
    {
        _monitor = monitor;
        _index = index;
        _search = search;
    }

    public Task StartAsync()
    {
        _monitor.OnChangeDetected += MonitorOnOnChangeDetected;
        _monitor.OnDeletionDetected += MonitorOnOnDeletionDetected;
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        Unregister();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Unregister();
    }

    private void Unregister()
    {
        _monitor.OnChangeDetected -= MonitorOnOnChangeDetected;
        _monitor.OnDeletionDetected -= MonitorOnOnDeletionDetected;
    }

    private void MonitorOnOnDeletionDetected(object sender, string path)
    {
        _index.RemoveFromIndexByPath(path);
        _index.FlushAndCommit();
        (_search as SearchAdapter)?.ResetCache();
    }

    private void MonitorOnOnChangeDetected(object sender, Repository e)
    {
        var repo = new RepositorySearchModel()
            {
                Path = e.Path,
                RepositoryName = e.Name,
                Tags = e.Tags.ToList(),
            };

        _index.ReIndexMediaFileAsync(repo);
        (_search as SearchAdapter)?.ResetCache();
    }
}