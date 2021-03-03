using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using RepoZ.Api.Common;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Common.Git.AutoFetch;
using RepoZ.Api.Common.Git.ProcessExecution;
using RepoZ.Api.Common.IO;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Mac;
using RepoZ.Api.Mac.IO;
using RepoZ.App.Mac.i18n;
using RepoZ.App.Mac.NativeSupport;
using RepoZ.App.Mac.NativeSupport.Git;
using RepoZ.Ipc;
using TinyIoC;
using TinySoup;
using TinySoup.Model;

namespace RepoZ.App.Mac
{
	[Register("AppDelegate")]
	public partial class AppDelegate : NSApplicationDelegate, INSPopoverDelegate, IRepositorySource
	{
		NSStatusItem _statusItem;
		NSPopover _pop;
		public static NSViewController _ctrl;

		private IRepositoryMonitor _repositoryMonitor;
		private NSObject _eventMonitor;
		private Timer _updateTimer;
		private IpcServer _ipcServer;

		public override void DidFinishLaunching(NSNotification notification)
		{
			var isRetina = NSScreen.MainScreen.BackingScaleFactor > 1.0;
			var isBigSurOrNewer = NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0));

			string statusItemImageName = $"StatusBarImage{(isBigSurOrNewer ? "Template" : "")}{(isRetina ? "@2x" : "")}.png";

			_statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
			_statusItem.Image = new NSImage(statusItemImageName) { Template = isBigSurOrNewer }; // defining as template will make the icon work in light/dark mode and reduced transparency (see #137)
			_statusItem.Target = this;
			_statusItem.Action = new ObjCRuntime.Selector("MenuAction");

			var container = TinyIoCContainer.Current;

			RegisterServices(container);
			UseRepositoryMonitor(container);
			PreloadRepositoryActions(container);

			_pop = new NSPopover();
			_pop.Behavior = NSPopoverBehavior.Transient;
			_pop.Delegate = this;
			_pop.ContentViewController = new PopupViewController();

			_eventMonitor = NSEvent.AddGlobalMonitorForEventsMatchingMask(NSEventMask.KeyDown, HandleGlobalEventHandler);

			_updateTimer = new Timer(CheckForUpdatesAsync, null, 5000, Timeout.Infinite);

			_ipcServer = new IpcServer(new DefaultIpcEndpoint(), this);
			_ipcServer.Start();
		}

		public override void WillTerminate(NSNotification notification)
		{
			_ipcServer?.Stop();
			_ipcServer?.Dispose();

			// Insert code here to tear down your application
			NSEvent.RemoveMonitor(_eventMonitor);
		}

		// Handler method definition
		[Export("MenuAction")]
		private void MenuAction()
		{
			if (_pop.Shown)
				_pop.Close();
			else
				_pop.Show(_statusItem.Button.Frame, _statusItem.Button, NSRectEdge.MaxYEdge);
		}

		private void RegisterServices(TinyIoCContainer container)
		{
			container.Register<IRepositoryInformationAggregator, DefaultRepositoryInformationAggregator>().AsSingleton();

			container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>().AsSingleton();
			container.Register<IRepositoryDetectorFactory, MacRepositoryDetectorFactory>().AsSingleton();
			container.Register<IRepositoryObserverFactory, MacRepositoryObserverFactory>().AsSingleton();
			container.Register<IPathCrawlerFactory, DefaultPathCrawlerFactory>().AsSingleton();

			container.Register<IAppDataPathProvider, MacAppDataPathProvider>();
			container.Register<IErrorHandler, UIErrorHandler>();
			container.Register<IRepositoryActionProvider, DefaultRepositoryActionProvider>();
			container.Register<IRepositoryReader, DefaultRepositoryReader>();
			container.Register<IRepositoryWriter, DefaultRepositoryWriter>();
			container.Register<IRepositoryStore, DefaultRepositoryStore>();
			container.Register<IPathProvider, MacDriveEnumerator>();
			container.Register<IPathCrawler, GravellPathCrawler>();
			container.Register<IPathSkipper, MacPathSkipper>();
			container.Register<IThreadDispatcher, MacThreadDispatcher>().AsSingleton();
			container.Register<IGitCommander, ProcessExecutingGitCommander>();
			container.Register<IAppSettingsService, FileAppSettingsService>();
			container.Register<IAutoFetchHandler, DefaultAutoFetchHandler>().AsSingleton();
			container.Register<IRepositoryIgnoreStore, DefaultRepositoryIgnoreStore>().AsSingleton();
			container.Register<IRepositoryIgnoreStore, DefaultRepositoryIgnoreStore>().AsSingleton();
			container.Register<IRepositoryActionConfigurationStore, DefaultRepositoryActionConfigurationStore>().AsSingleton();
			container.Register<ITranslationService, ResourceDictionaryTranslationService>();
		}

		private void UseRepositoryMonitor(TinyIoCContainer container)
		{
			_repositoryMonitor = container.Resolve<IRepositoryMonitor>();
			_repositoryMonitor.Observe();
		}

		private void PreloadRepositoryActions(TinyIoCContainer container)
		{
			var store = container.Resolve<IRepositoryActionConfigurationStore>();
			store.Preload();
		}

		private async void CheckForUpdatesAsync(object state)
		{
			var bundleVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();

			var request = new UpdateRequest()
				.WithNameAndVersionFromEntryAssembly()
				.WithVersion(bundleVersion)
				.AsAnonymousClient()
				.OnChannel("stable")
				.OnPlatform(new OperatingSystemIdentifier().WithSuffix("(Mac)"));

			var client = new WebSoupClient();
			var updates = await client.CheckForUpdatesAsync(request);

			AvailableUpdate = updates.FirstOrDefault();

			_updateTimer.Change((int)TimeSpan.FromHours(2).TotalMilliseconds, Timeout.Infinite);
		}

		void HandleGlobalEventHandler(NSEvent globalEvent)
		{
			if (globalEvent.KeyCode == (ushort)NSKey.R)
			{
				var holdsOption = globalEvent.ModifierFlags.HasFlag(NSEventModifierMask.AlternateKeyMask);
				var holdsCommand = globalEvent.ModifierFlags.HasFlag(NSEventModifierMask.CommandKeyMask);

				if (holdsOption && holdsCommand)
					MenuAction();
			}
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
					Path = r.Path
				})
				.ToArray();
		}

		public static AvailableVersion AvailableUpdate { get; private set; }
	}
}
