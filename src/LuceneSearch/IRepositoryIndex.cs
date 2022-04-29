namespace LuceneSearch;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucene.Net.Search;

internal interface IRepositoryIndex
{
    Task<bool> ReIndexMediaFileAsync(RepositorySearchModel data);

    int Count(Query query = null, Filter filter = null);

    RepositorySearchResult Search(Guid guid);

    List<RepositorySearchResult> Search(string queryString, SearchOperator searchMode, out int totalHits);

    List<RepositorySearchResult> Search(Query query, Filter filter, out int totalHits);

    void RemoveFromIndexByPath(string path);

    void FlushAndCommit();
}