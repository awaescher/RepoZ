using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace RepoZ.Ipc
{
	public partial class IpcClient
	{
		private string _answer;
		private Repository[] _repositories;

		public IpcClient(IIpcEndpoint endpointProvider)
		{
			EndpointProvider = endpointProvider ?? throw new ArgumentNullException(nameof(endpointProvider));
		}

		public Result GetRepositories() => GetRepositories("list:.*");

		public Result GetRepositories(string query)
		{
			var watch = Stopwatch.StartNew();

			_answer = null;
			_repositories = null;

			using (var client = new RequestSocket())
			{
				client.Connect(EndpointProvider.Address);

				byte[] load = Encoding.UTF8.GetBytes(query);
				client.SendFrame(load);
				client.ReceiveReady += ClientOnReceiveReady;

				client.Poll(TimeSpan.FromMilliseconds(3000));

				client.ReceiveReady -= ClientOnReceiveReady;
				client.Disconnect(EndpointProvider.Address);
			}

			watch.Stop();

			return new Result()
			{
				Answer = _answer ?? GetErrorMessage(),
				DurationMilliseconds = watch.ElapsedMilliseconds,
				Repositories = _repositories ?? new Repository[0]
			};
		}

		private string GetErrorMessage()
		{
			var isRepoZRunning = Process.GetProcessesByName("RepoZ").Any();
			return isRepoZRunning
				? $"RepoZ seems to be running but does not answer.\nIt seems that it could not listen on {EndpointProvider.Address}.\nI don't know anything better than recommending a reboot. Sorry."
				: "RepoZ seems not to be running :(";
		}

		private void ClientOnReceiveReady(object sender, NetMQSocketEventArgs e)
		{
			_answer = Encoding.UTF8.GetString(e.Socket.ReceiveFrameBytes());

			_repositories = _answer.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
				.Select(s => Repository.FromString(s))
				.Where(r => r != null)
				.OrderBy(r => r.Name)
				.ToArray();
		}

		public IIpcEndpoint EndpointProvider { get; }

		public class Result
		{
			public string Answer { get; set; }
			public long DurationMilliseconds { get; set; }
			public Repository[] Repositories { get; set; }
		}
	}
}