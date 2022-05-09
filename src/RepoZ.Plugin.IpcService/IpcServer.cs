namespace RepoZ.Plugin.IpcService
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using NetMQ;
    using NetMQ.Sockets;
    using RepoZ.Ipc;

    internal class IpcServer : IDisposable
    {
        private ResponseSocket _socketServer;

        public IpcServer(IIpcEndpoint endpointProvider, IRepositorySource repositorySource)
        {
            EndpointProvider = endpointProvider ?? throw new ArgumentNullException(nameof(endpointProvider));
            RepositorySource = repositorySource ?? throw new ArgumentNullException(nameof(repositorySource));
        }

        public void Start()
        {
            Task.Run(() => StartInternal());
        }

        private void StartInternal()
        {
            _socketServer = new ResponseSocket(EndpointProvider.Address);

            while (true)
            {
                var load = _socketServer.ReceiveFrameBytes(out bool hasMore);

                var message = Encoding.UTF8.GetString(load);

                if (string.IsNullOrEmpty(message))
                {
                    return;
                }

                if (message.StartsWith("list:", StringComparison.Ordinal))
                {
                    var repositoryNamePattern = message.Substring("list:".Length);

                    var answer = "(no repositories found)";
                    try
                    {
                        Ipc.Repository[] repos = RepositorySource.GetMatchingRepositories(repositoryNamePattern);
                        if (repos.Any())
                        {
                            var serializedRepositories = repos
                                                         .Where(r => r != null)
                                                         .Select(r => r.ToString());

                            answer = string.Join(Environment.NewLine, serializedRepositories);
                        }
                    }
                    catch (Exception ex)
                    {
                        answer = ex.Message;
                    }

                    _socketServer.SendFrame(Encoding.UTF8.GetBytes(answer));
                }

                Thread.Sleep(100);
            }
        }

        public void Stop()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _socketServer?.Disconnect(EndpointProvider.Address);
                _socketServer?.Dispose();
                _socketServer = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public IIpcEndpoint EndpointProvider { get; }

        public IRepositorySource RepositorySource { get; }
    }
}