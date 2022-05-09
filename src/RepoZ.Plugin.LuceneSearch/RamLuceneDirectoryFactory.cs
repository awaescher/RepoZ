namespace RepoZ.Plugin.LuceneSearch;

using Lucene.Net.Store;

internal class RamLuceneDirectoryFactory : ILuceneDirectoryFactory
{
    public Directory Create()
    {
        return new RAMDirectory();
    }
}