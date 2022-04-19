namespace Specs.Ipc
{
    using RepoZ.Ipc;

    class TestIpcEndpoint : IIpcEndpoint
    {
        public string Address => "tcp://localhost:18182";
    }
}