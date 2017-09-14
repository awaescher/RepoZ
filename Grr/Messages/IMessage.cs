namespace grr.Messages
{
	public interface IMessage
	{
		string GetRemoteCommand();

		void Execute(Repository[] repositories);
	}
}