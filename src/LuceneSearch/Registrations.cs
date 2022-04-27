namespace LuceneSearch;

using System;
using RepoZ.Api;

public static class Registrations
{
    public static void RegisterInternals(Action<Type, Type, bool> action)
    {
        action.Invoke(typeof(IRepositorySearch), typeof(SearchAdapter), true);
        action.Invoke(typeof(ILuceneDirectoryFactory), typeof(RamLuceneDirectoryFactory), true);

        action.Invoke(typeof(IRepositorySearch), typeof(SearchAdapter), true);
        action.Invoke(typeof(IRepositoryIndex), typeof(RepositoryIndex), true);
        action.Invoke(typeof(EventToLuceneHandler), typeof(EventToLuceneHandler), true);
        action.Invoke(typeof(LuceneDirectoryInstance), typeof(LuceneDirectoryInstance), true);
        action.Invoke(typeof(RepositoryIndex), typeof(RepositoryIndex), true);
    }

    public static void Start(Func<Type, object> func)
    {
        _ = func.Invoke(typeof(EventToLuceneHandler));
    }
}