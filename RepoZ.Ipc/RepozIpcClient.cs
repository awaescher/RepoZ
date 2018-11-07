using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TinyIpc.Messaging;

namespace RepoZ.Ipc
{
	public class RepozIpcClient
	{
		private TinyMessageBus _bus;
		private Repository[] _repositories;
		private string _answer;

		public Result GetRepositories() => GetRepositories("list:.*");

		public Result GetRepositories(string query)
		{
			_repositories = null;

			_bus = new TinyMessageBus("RepoZ-ipc");
			_bus.MessageReceived += _bus_MessageReceived;

			byte[] load = Encoding.UTF8.GetBytes(query);
			_bus.PublishAsync(load);

			var watch = Stopwatch.StartNew();

			while (_answer == null && watch.ElapsedMilliseconds <= 3000)
			{ /* ... wait ... */ }

			watch.Stop();

			_bus?.Dispose();

			return new Result()
			{
				Answer = _answer ?? "RepoZ seems not to be running :(",
				DurationMilliseconds = watch.ElapsedMilliseconds,
				Repositories = _repositories ?? new Repository[0]
			};
		}

		private void _bus_MessageReceived(object sender, TinyMessageReceivedEventArgs e)
		{
			var answer = Encoding.UTF8.GetString(e.Message);
			_answer = answer;
			_repositories = answer.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
				.Select(s => Repository.FromString(s))
				.Where(r => r != null)
				.OrderBy(r => r.Name)
				.ToArray();
		}

		public class Result
		{
			public string Answer { get; set; }
			public long DurationMilliseconds { get; set; }
			public Repository[] Repositories { get; set; }
		}
	}
}