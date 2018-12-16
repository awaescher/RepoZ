using RepoZ.Api.Common.Git.ProcessExecution;
using System;
using System.IO;

namespace RepoZ.Api.Common.Git
{
	public interface IGitCommander
	{
		string Command(Api.Git.Repository repository, params string[] command);
		string CommandOneline(Api.Git.Repository repository, params string[] command);
		void CommandNoisy(Api.Git.Repository repository, params string[] command);
		void CommandOutputPipe(Api.Git.Repository repository, Action<TextReader> func, params string[] command);
		void CommandInputPipe(Api.Git.Repository repository, Action<TextWriter> action, params string[] command);
		void CommandInputOutputPipe(Api.Git.Repository repository, Action<TextWriter, TextReader> interact, params string[] command);
		void WrapGitCommandErrors(string exceptionMessage, Action action);
		void Close(GitProcess process);
	}
}
