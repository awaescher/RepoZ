namespace RepoZ.Plugin.LuceneSearch;

using System.Collections.Generic;

internal class RepositorySearchModel
{
    public string RepositoryName { get; set; }

    public string Path { get; set; }

    public List<string> Tags { get; set; }
}