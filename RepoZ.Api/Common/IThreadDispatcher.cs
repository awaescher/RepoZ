namespace RepoZ.Api.Common
{
    using System;

    public interface IThreadDispatcher
    {
        void Invoke(Action act);
    }
}