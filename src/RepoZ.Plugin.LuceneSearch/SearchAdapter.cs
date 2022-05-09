namespace RepoZ.Plugin.LuceneSearch;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoZ.Api;

internal class SearchAdapter : IRepositorySearch
{
    private readonly IRepositoryIndex _index;
    private List<RepositorySearchResult> _cache  = new List<RepositorySearchResult>();
    private string _query = string.Empty;
    private DateTime _now = DateTime.MinValue;
    private readonly object _l = new();

    public SearchAdapter(IRepositoryIndex index)
    {
        _index = index;
    }

    public IEnumerable<string> Search(string query)
    {
        if (_now > DateTime.UtcNow)
        {
            if (_query.Equals(query))
            {
                return _cache.Select(x => x.Path);
            }
        }

        lock (_l)
        {
            if (_now > DateTime.UtcNow)
            {
                if (_query.Equals(query))
                {
                    return _cache.Select(x => x.Path);
                }
            }

            List<RepositorySearchResult> results = _index.Search(query, SearchOperator.And,out var _);
            _cache = results;
            _now = DateTime.UtcNow.AddSeconds(50);
            _query = query;

            return results.Select(x => x.Path);
        }
    }

    public void ResetCache()
    {
        _now = DateTime.MinValue;
    }
}