namespace RepoZ.Plugin.LuceneSearch;

using Lucene.Net.Store;

internal class LuceneDirectoryInstance
{
    private readonly ILuceneDirectoryFactory _factory;
    private Directory _instance;

    public LuceneDirectoryInstance(ILuceneDirectoryFactory factory)
    {
        _factory = factory;
    }

    public Directory Instance => _instance ??= _factory.Create();
}