namespace RepoZ.Api.Common
{
    using System;

    public interface IClock
    {
        DateTime Now { get; }
    }
}