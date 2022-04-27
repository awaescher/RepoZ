namespace LuceneSearch;

using Lucene.Net.Store;

internal interface ILuceneDirectoryFactory
{
    Directory Create();
}