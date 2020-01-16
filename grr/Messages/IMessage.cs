using RepoZ.Ipc;

namespace grr.Messages
{
	public interface IMessage
	{
		string GetRemoteCommand();

		void Execute(Repository[] repositories);

		bool HasRemoteCommand { get; }

		bool ShouldWriteRepositories(Repository[] repositories);
	}
}