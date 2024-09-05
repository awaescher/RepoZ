namespace RepoZ.Ipc
{
    public class DefaultIpcEndpoint : IIpcEndpoint
    {
#if DEBUG
        public string Address => "tcp://localhost:18183";
#else
        public string Address => "tcp://localhost:18182";
#endif
    }
}