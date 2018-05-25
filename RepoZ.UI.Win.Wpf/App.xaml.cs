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
using Hardcodet.Wpf.TaskbarNotification;
using TinySoup.Model;
using TinySoup;

namespace RepoZ.UI.Win.Wpf
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static Timer _explorerUpdateTimer;
		private static Timer _updateTimer;
		private HotKey _hotkey;
		private static WindowsExplorerHandler _explorerHandler;
		private static IRepositoryMonitor _repositoryMonitor;
		private static TinyMessageBus _bus;
		private TaskbarIcon _notifyIcon;

		[STAThread]
		public static void Main()
		{
			var app = new App();
			app.InitializeComponent();
			app.Run();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			_notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

			var container = TinyIoCContainer.Current;

			RegisterServices(container);

			UseRepositoryMonitor(container);
			UseExplorerHandler(container);

			_bus = new TinyMessageBus("RepoZ-ipc");
			_bus.MessageReceived += Bus_MessageReceived;

			_updateTimer = new Timer(CheckForUpdatesAsync, null, 5000, Timeout.Infinite);

			_hotkey = new HotKey(47110815);
			_hotkey.Register(container.Resolve<MainWindow>(), HotKey.VK_R, HotKey.MOD_ALT | HotKey.MOD_CTRL, OnHotKeyPressed);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			_hotkey.Unregister();

			_explorerUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);

			var explorerHandler = TinyIoCContainer.Current.Resolve<WindowsExplorerHandler>();
			explorerHandler.CleanTitles();

			_notifyIcon.Dispose();

			base.OnExit(e);
		}

		protected static void RegisterServices(TinyIoCContainer container)
		{
			container.Register<MainWindow>().AsSingleton();

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
			container.Register<IGitCommander, WindowsGitCommander>();
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

		private async void CheckForUpdatesAsync(object state)
		{
			var request = new UpdateRequest()
				.WithNameAndVersionFromEntryAssembly()
				.AsAnonymousClient()
				.OnChannel("stable")
				.OnPlatform(new OperatingSystemIdentifier().WithSuffix("(WPF)"));

			var client = new WebSoupClient();
			var updates = await client.CheckForUpdatesAsync(request);

			AvailableUpdate = updates.FirstOrDefault();

			_updateTimer.Change((int)TimeSpan.FromHours(2).TotalMilliseconds, Timeout.Infinite);
		}

		protected static void RefreshTimerCallback(object state)
		{
			_explorerHandler.UpdateTitles();
			_explorerUpdateTimer.Change(500, Timeout.Infinite);
		}

		private void OnHotKeyPressed()
		{
			(Application.Current.MainWindow as MainWindow)?.ShowAndActivate();
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

		public static AvailableVersion AvailableUpdate { get; private set; }
	}
}
