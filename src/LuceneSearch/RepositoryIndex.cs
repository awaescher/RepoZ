namespace RepoZ.Plugin.LuceneSearch;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Directory = Lucene.Net.Store.Directory;

internal enum SearchOperator
{
    And,

    Or,
}

internal class RepositoryIndex : IRepositoryIndex, IDisposable
{
    private const string KEY_ID = "id";
    // private const string KeyVersion = "version";
    private const string KEY_REPOSITORY_NAME = "name";
    private const string KEY_REPOSITORY_PATH = "path";
    private const string KEY_TAG = "tag";
    
    private readonly Analyzer _analyzer;
    private readonly IndexWriter _indexWriter;
    private readonly SearcherManager _searcherManager;
    private readonly Directory _indexDirectory;
    private readonly object _syncLock = new();

    public RepositoryIndex(LuceneDirectoryInstance indexDirectoryFactory)
    {
        _indexDirectory = indexDirectoryFactory.Instance;

        _analyzer = new StandardAnalyzer(LuceneNetVersion.VERSION);
        /*
                _analyzer = new PerFieldAnalyzerWrapper(
                                                        new HtmlStripAnalyzer(LuceneNetVersion.VERSION),
                                                        new Dictionary<string, Analyzer>
                                                            {
                                                                {
                                                                    "owner",
                                                                    Analyzer.NewAnonymous((fieldName, reader) =>
                                                                                          {
                                                                                              var source = new KeywordTokenizer(reader);
                                                                                              TokenStream result = new ASCIIFoldingFilter(source);
                                                                                              result = new LowerCaseFilter(LuceneNetVersion.VERSION, result);
                                                                                              return new TokenStreamComponents(source, result);
                                                                                          })
                                                                },
                                                                {
                                                                    "name",
                                                                    Analyzer.NewAnonymous((fieldName, reader) =>
                                                                                          {
                                                                                              var source = new StandardTokenizer(LuceneNetVersion.VERSION, reader);
                                                                                              TokenStream result = new WordDelimiterFilter(LuceneNetVersion.VERSION, source, ~WordDelimiterFlags.STEM_ENGLISH_POSSESSIVE, CharArraySet.EMPTY_SET);
                                                                                              result = new ASCIIFoldingFilter(result);
                                                                                              result = new LowerCaseFilter(LuceneNetVersion.VERSION, result);
                                                                                              return new TokenStreamComponents(source, result);
                                                                                          })
                                                                }
                                                            });
    */

        var indexWriterConfig = new IndexWriterConfig(LuceneNetVersion.VERSION, _analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND,
                RAMBufferSizeMB = 256.0,
            };

        _indexWriter = new IndexWriter(_indexDirectory, indexWriterConfig);
        _searcherManager = new SearcherManager(_indexWriter, true, null);
    }

    public Task<bool> ReIndexMediaFileAsync(RepositorySearchModel data)
    {
        RemoveFromIndexByPath(data.Path);
        // RemoveFromIndexByGuid(data.Id);

        var doc = new Document
            {
                // file information
                // new StringField(KEY_ID, data.Id.ToString(), Field.Store.YES),
                // new NumericDocValuesField(KeyVersion, data.Version),
                // new StoredField(KeyVersion, data.Version),
                new TextField(KEY_REPOSITORY_NAME, data.RepositoryName ?? string.Empty, Field.Store.YES),
                new StringField(KEY_REPOSITORY_PATH, data.Path ?? string.Empty, Field.Store.YES),
            };

        foreach (var tag in data.Tags ?? new List<string>())
        {
            doc.Add(new TextField(KEY_TAG, tag, Field.Store.YES));
        }

        if (_indexWriter.Config.OpenMode == OpenMode.CREATE)
        {
            // New index, so we just add the document (no old document can be there):
            _indexWriter.AddDocument(doc);
        }
        else
        {
            // Existing index (an old copy of this document may have been indexed) so
            // we use updateDocument instead to replace the old one matching the exact
            // path, if present:
            _indexWriter.UpdateDocument(new Term(KEY_REPOSITORY_PATH, data.Path), doc);
        }

        FlushAndCommit();
        return Task.FromResult(true);
    }

    public void FlushAndCommit()
    {
        _indexWriter.Flush(true, true);
        _indexWriter.Commit();
        // expensive
        // _indexWriter.ForceMerge(1);
    }

    public int Count(Query query = null, Filter filter = null)
    {
        // Execute the search with a fresh indexSearcher
        _searcherManager.MaybeRefreshBlocking();

        IndexSearcher searcher = _searcherManager.Acquire();

        try
        {
            query ??= new MatchAllDocsQuery();

            TopDocs topDocs = filter == null
                ? searcher.Search(query, 1)
                : searcher.Search(query, filter, 1);

            return topDocs.TotalHits;
        }
        catch (Exception)
        {
            // do nothing
        }
        finally
        {
            _searcherManager.Release(searcher);
        }

        throw new Exception("No items found.");
    }

    public RepositorySearchResult Search(Guid guid)
    {
        if (guid == Guid.Empty)
        {
            return null;
        }

        var term = new Term(KEY_ID, guid.ToString());
        var query = new TermQuery(term);
        return Search(query, null, out _).SingleOrDefault();
    }
    
    public List<RepositorySearchResult> Search(string queryString, SearchOperator searchMode, out int totalHits)
    {
        lock (_syncLock)
        {
            // Parse the query - assuming it's not a single term but an actual query string
            // the QueryParser used is using the same analyzer used for indexing
            Query query = CreateQueryParser(MapOperator(searchMode)).Parse(queryString);

            return Search(query, null, out totalHits);
        }
    }

    public List<RepositorySearchResult> Search(Query query, Filter filter, out int totalHits)
    {
        var results = new List<RepositorySearchResult>();
        totalHits = 0;

        // Execute the search with a fresh indexSearcher
        _searcherManager.MaybeRefreshBlocking();

        IndexSearcher searcher = _searcherManager.Acquire();

        try
        {
            TopDocs topDocs = filter == null
                ? searcher.Search(query, 1000)
                : searcher.Search(query, filter, 1000);

            totalHits = topDocs.TotalHits;

            foreach (var result in topDocs.ScoreDocs)
            {
                Document doc = searcher.Doc(result.Doc);

                // Results are automatically sorted by relevance
                var item = new RepositorySearchResult(result.Score)
                    {
                        // Id = GetId(doc),
                        RepositoryName = doc.GetField(KEY_REPOSITORY_NAME)?.GetStringValue(),
                        Path = doc.GetField(KEY_REPOSITORY_PATH)?.GetStringValue(),
                        Tags = doc.GetValues(KEY_TAG)?.ToList() ?? new List<string>(),
                    };

                results.Add(item);
            }
        }
        catch (Exception e)
        {
            // do nothing
            Debug.Fail("Should not happen" + e.Message);
        }
        finally
        {
            _searcherManager.Release(searcher);
        }

        return results;
    }

    public void Dispose()
    {
        _analyzer.Dispose();
        _indexWriter.Dispose();
        _searcherManager.Dispose();
        _indexDirectory.Dispose();
    }

    public static Guid GetId([NotNull] Document doc)
    {
        var guidString = doc.GetField(KEY_ID)?.GetStringValue();
        if (string.IsNullOrWhiteSpace(guidString))
        {
            return Guid.Empty;
        }

        return Guid.TryParse(guidString, out var id) ? id : Guid.Empty;
    }

    // todo
    private static readonly string[] _defaultQueryFields = new[] { KEY_REPOSITORY_NAME, KEY_TAG, };

    private MultiFieldQueryParser CreateQueryParser(Operator defaultOperator)
    {
        var result = new MultiFieldQueryParser(LuceneNetVersion.VERSION, _defaultQueryFields, _analyzer)
            {
                DefaultOperator = defaultOperator,
            };
        return result;
    }

    public void RemoveFromIndexByPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        DeleteByTerm(new Term(KEY_REPOSITORY_PATH, path));
    }

    public void RemoveFromIndexByGuid(Guid guid)
    {
        if (guid == Guid.Empty)
        {
            return;
        }

        DeleteByTerm(new Term(KEY_ID, guid.ToString()));
    }

    private static Operator MapOperator(SearchOperator searchOperator)
    {
        return searchOperator switch
            {
                SearchOperator.And => Operator.AND,
                SearchOperator.Or => Operator.OR,
                _ => throw new ArgumentOutOfRangeException(nameof(searchOperator), searchOperator, null)
            };
    }

    private void DeleteByTerm(Term term)
    {
        try
        {
            _indexWriter.DeleteDocuments(term);
            _indexWriter.Flush(true, true);
            _indexWriter.Commit();
        }
        catch (OutOfMemoryException)
        {
            _indexWriter.Dispose();
            throw;
        }
    }
}