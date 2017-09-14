using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using RepoZ.Api.Common;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Common.IO;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Win.Git;
using RepoZ.Api.Win.IO;
using RepoZ.Api.Win.PInvoke.Explorer;
using TinyIoC;
using TinyIpc.Messaging;
using System.Text.RegularExpressions;

namespace RepoZ.UI.Win.Wpf
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static Timer _explorerUpdateTimer;
		private static WindowsExplorerHandler _explorerHandler;
		private static IRepositoryMonitor _repositoryMonitor;
		private static TinyMessageBus _bus;

		[STAThread]
		public static void Main(string[] args)
		{
			bool noUI = args?.Any(arg => arg.EndsWith("-noui", StringComparison.OrdinalIgnoreCase)) ?? false;
			var container = TinyIoCContainer.Current;

			var application = new App();
			application.InitializeComponent();

			RegisterServices(container);

			UseRepositoryMonitor(container);
			UseExplorerHandler(container);

			_bus = new TinyMessageBus("RepoZ-ipc");
			_bus.MessageReceived += Bus_MessageReceived;

			if (noUI)
			{
				application.Run();
			}
			else
			{
				var form = container.Resolve<MainWindow>();
				application.Run(form);
			}

		}

		private static void Bus_MessageReceived(object sender, TinyMessageReceivedEventArgs e)
		{
			string message = Encoding.UTF8.GetString(e.Message);

			if (string.IsNullOrEmpty(message))
				return;

			if (message.StartsWith("list:"))
			{
				string repositoryNamePattern = message.Substring("list:".Length);
				var bus = (TinyMessageBus)sender;

				string answer = "(no repositories found)";
				try
				{
					var aggregator = TinyIoCContainer.Current.Resolve<IRepositoryInformationAggregator>();
					var repos = aggregator.Repositories
						.Where(r => string.IsNullOrEmpty(repositoryNamePattern) || Regex.IsMatch(r.Name, repositoryNamePattern, RegexOptions.IgnoreCase))
						.Select(r => $"{r.Name}|{r.BranchWithStatus}|{r.Path}")
						.ToArray();

					if (repos.Any())
						answer = string.Join(Environment.NewLine, repos);
				}
				catch (Exception ex)
				{
					answer = ex.Message;
				}

				bus.PublishAsync(Encoding.UTF8.GetBytes(answer));
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			_explorerUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);

			var explorerHandler = TinyIoCContainer.Current.Resolve<WindowsExplorerHandler>();
			explorerHandler.CleanTitles();

			base.OnExit(e);
		}

		protected static void RegisterServices(TinyIoCContainer container)
		{
			container.Register<IRepositoryInformationAggregator, DefaultRepositoryInformationAggregator>().AsSingleton();

			container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>().AsSingleton();
			container.Register<WindowsExplorerHandler>().AsSingleton();
			container.Register<IRepositoryDetectorFactory, DefaultRepositoryDetectorFactory>().AsSingleton();
			container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>().AsSingleton();
			container.Register<IPathCrawlerFactory, DefaultPathCrawlerFactory>().AsSingleton();

			container.Register<IErrorHandler, UIErrorHandler>();
			container.Register<IRepositoryActionProvider, WindowsRepositoryActionProvider>();
			container.Register<IRepositoryReader, DefaultRepositoryReader>();
			container.Register<IRepositoryWriter, DefaultRepositoryWriter>();
			container.Register<IRepositoryStore, WindowsRepositoryStore>();
			container.Register<IPathProvider, DefaultDriveEnumerator>();
			container.Register<IPathCrawler, GravellPathCrawler>();
			container.Register<IPathSkipper, WindowsPathSkipper>();
			container.Register<IThreadDispatcher, WpfThreadDispatcher>().AsSingleton();

		}

		protected static void UseRepositoryMonitor(TinyIoCContainer container)
		{
			var repositoryInformationAggregator = container.Resolve<IRepositoryInformationAggregator>();
			_repositoryMonitor = container.Resolve<IRepositoryMonitor>();
			_repositoryMonitor.Observe();
		}

		protected static void UseExplorerHandler(TinyIoCContainer container)
		{
			_explorerHandler = container.Resolve<WindowsExplorerHandler>();
			_explorerUpdateTimer = new Timer(RefreshTimerCallback, null, 1000, Timeout.Infinite);
		}

		protected static void RefreshTimerCallback(Object state)
		{
			_explorerHandler.UpdateTitles();
			_explorerUpdateTimer.Change(500, Timeout.Infinite);
		}
	}
}
