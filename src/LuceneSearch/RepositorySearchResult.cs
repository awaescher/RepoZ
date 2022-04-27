namespace LuceneSearch;

internal class RepositorySearchResult : RepositorySearchModel
{
    internal RepositorySearchResult(float score)
    {
        Score = score;
    }

    public float Score { get; set; }
}