using System;
using System.IO;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Git;

namespace RepoZ.Api.Mac.Git
{
    public class MacGitCommander : IGitCommander
    {
        // look at all those TODOs I have ...

        public string Command(Repository repository, params string[] command)
        {
            throw new NotImplementedException();
        }

        public void CommandInputOutputPipe(Repository repository, Action<TextWriter, TextReader> interact, params string[] command)
        {
            throw new NotImplementedException();
        }

        public void CommandInputPipe(Repository repository, Action<TextWriter> action, params string[] command)
        {
            throw new NotImplementedException();
        }

        public void CommandNoisy(Repository repository, params string[] command)
        {
            throw new NotImplementedException();
        }

        public string CommandOneline(Repository repository, params string[] command)
        {
            throw new NotImplementedException();
        }

        public void CommandOutputPipe(Repository repository, Action<TextReader> func, params string[] command)
        {
            throw new NotImplementedException();
        }

        public void WrapGitCommandErrors(string exceptionMessage, Action action)
        {
            throw new NotImplementedException();
        }
    }
}
