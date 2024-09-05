using RepoZ.Ipc;

namespace Specs.Ipc
{
    class TestIpcEndpoint : IIpcEndpoint
    {

#if DEBUG
        public string Address => "tcp://localhost:18183";
#else
        public string Address => "tcp://localhost:18182";
#endif
    }
}
