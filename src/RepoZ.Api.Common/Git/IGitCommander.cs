namespace RepoZ.Api.Common.Git
{
    using System;

    public interface IGitCommander
    {
        string Command(Api.Git.Repository repository, params string[] command);

        string CommandOneline(Api.Git.Repository repository, params string[] command);

        void CommandNoisy(Api.Git.Repository repository, params string[] command);

        void CommandOutputPipe(Api.Git.Repository repository, Action<string> handleOutput, params string[] command);
    }
}