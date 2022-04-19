namespace grr.Messages
{
    using RepoZ.Ipc;

    public interface IMessage
    {
        string GetRemoteCommand();

        void Execute(Repository[] repositories);

        bool HasRemoteCommand { get; }

        bool ShouldWriteRepositories(Repository[] repositories);
    }
}