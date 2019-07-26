using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RepoZ.Api.Common;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Common.IO;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Win.IO;
using RepoZ.Api.Win.PInvoke.Explorer;
using TinyIoC;
using System.Text.RegularExpressions;
using Hardcodet.Wpf.TaskbarNotification;
using TinySoup.Model;
using TinySoup;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.Git.AutoFetch;
using RepoZ.Api.Common.Git.ProcessExecution;
using RepoZ.Ipc;

namespace RepoZ.App.Win
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application, IRepositorySource
	{
		private static Timer _explorerUpdateTimer;
		private static Timer _updateTimer;
		private HotKey _hotkey;
		private static WindowsExplorerHandler _explorerHandler;
		private static IRepositoryMonitor _repositoryMonitor;
		private TaskbarIcon _notifyIcon;
		private IpcServer _ipcServer;

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

			// Ensure the current culture passed into bindings is the OS culture.
			// By default, WPF uses en-US as the culture, regardless of the system settings.
			// see: https://stackoverflow.com/a/520334/704281
			FrameworkElement.LanguageProperty.OverrideMetadata(
				typeof(FrameworkElement),
				new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));

			_notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

			var container = TinyIoCContainer.Current;

			RegisterServices(container);

			UseRepositoryMonitor(container);
			UseExplorerHandler(container);

			_updateTimer = new Timer(async state => await CheckForUpdatesAsync(), null, 5000, Timeout.Infinite);

			// We noticed that the hotkey registration causes a high CPU utilization if the window was not shown before.
			// To fix this, we need to make the window visible in EnsureWindowHandle() but we set the opacity to 0.0 to prevent flickering
			var window = container.Resolve<MainWindow>();
			EnsureWindowHandle(window);

			_hotkey = new HotKey(47110815);
			_hotkey.Register(window, HotKey.VK_R, HotKey.MOD_ALT | HotKey.MOD_CTRL, OnHotKeyPressed);

			_ipcServer = new IpcServer(new DefaultIpcEndpoint(), this);
			_ipcServer.Start();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			_ipcServer?.Stop();
			_ipcServer?.Dispose();

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

			container.Register<IAppDataPathProvider, DefaultAppDataPathProvider>();
			container.Register<IErrorHandler, UIErrorHandler>();
			container.Register<IRepositoryActionProvider, WindowsRepositoryActionProvider>();
			container.Register<IRepositoryReader, DefaultRepositoryReader>();
			container.Register<IRepositoryWriter, DefaultRepositoryWriter>();
			container.Register<IRepositoryStore, DefaultRepositoryStore>();
			container.Register<IPathProvider, DefaultDriveEnumerator>();
			container.Register<IPathCrawler, GravellPathCrawler>();
			container.Register<IPathSkipper, WindowsPathSkipper>();
			container.Register<IThreadDispatcher, WpfThreadDispatcher>().AsSingleton();
			container.Register<IGitCommander, ProcessExecutingGitCommander>();
			container.Register<IAppSettingsService, FileAppSettingsService>();
			container.Register<IAutoFetchHandler, DefaultAutoFetchHandler>().AsSingleton();
			container.Register<IRepositoryIgnoreStore, DefaultRepositoryIgnoreStore>().AsSingleton();
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

		private async Task CheckForUpdatesAsync()
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

		private void EnsureWindowHandle(Window window)
		{
			// We noticed that the hotkey registration at app start causes a high CPU utilization if the main window was not shown before.
			// To fix this, we need to make the window visible. However, to prevent flickering we move the window out of the screen bounds to show and hide it.

			var originalLeft = window.Left;

			try
			{
				window.Left = -9999;
				window.Show();
				window.Hide();
			}
			finally
			{
				Task.Delay(50).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => window.Left = originalLeft)));
			}
		}

		private void OnHotKeyPressed()
		{
			(Application.Current.MainWindow as MainWindow)?.ShowAndActivate();
		}

		public Ipc.Repository[] GetMatchingRepositories(string repositoryNamePattern)
		{
			var aggregator = TinyIoCContainer.Current.Resolve<IRepositoryInformationAggregator>();
			return aggregator.Repositories
				.Where(r => r.MatchesRegexFilter(repositoryNamePattern))
				.Select(r => new Ipc.Repository
				{
					Name = r.Name,
					BranchWithStatus = r.BranchWithStatus,
					HasUnpushedChanges = r.HasUnpushedChanges,
					Path = r.Path
				})
				.ToArray();
		}

		public static AvailableVersion AvailableUpdate { get; private set; }
	}
}
