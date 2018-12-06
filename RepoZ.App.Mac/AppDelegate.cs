using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using NetMQ;
using NetMQ.Sockets;
using RepoZ.Api.Common;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Common.IO;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Mac;
using RepoZ.Api.Mac.Git;
using RepoZ.Api.Mac.IO;
using RepoZ.App.Mac.NativeSupport;
using RepoZ.App.Mac.NativeSupport.Git;
using TinyIoC;
using TinySoup;
using TinySoup.Model;

namespace RepoZ.App.Mac
{
    [Register("AppDelegate")]
    public partial class AppDelegate : NSApplicationDelegate, INSPopoverDelegate
    {
        NSStatusItem _statusItem;
        NSPopover _pop;
        public static NSViewController _ctrl;

        private IRepositoryMonitor _repositoryMonitor;
        private NSObject _eventMonitor;
        private Timer _updateTimer;
        private ResponseSocket _socketServer;

        public override void DidFinishLaunching(NSNotification notification)
        {
            var isRetina = NSScreen.MainScreen.BackingScaleFactor > 1.0;
            string statusItemImageName = $"StatusBarImage{(isRetina ? "@2x" : "")}.png";

            _statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
            _statusItem.Image = new NSImage(statusItemImageName);
            _statusItem.Target = this;
            _statusItem.Action = new ObjCRuntime.Selector("MenuAction");

            var container = TinyIoCContainer.Current;
            RegisterServices(container);
            UseRepositoryMonitor(container);

            _pop = new NSPopover();
            _pop.Behavior = NSPopoverBehavior.Transient;
            _pop.Delegate = this;
            _pop.ContentViewController = new PopupViewController();

            _eventMonitor = NSEvent.AddGlobalMonitorForEventsMatchingMask(NSEventMask.KeyDown, HandleGlobalEventHandler);

            _updateTimer = new Timer(CheckForUpdatesAsync, null, 5000, Timeout.Infinite);

            Task.Run(() => ListenForSocketRequests());
        }

        public override void WillTerminate(NSNotification notification)
        {
            _socketServer?.Disconnect(Ipc.RepoZIpcEndpoint.Address);
            _socketServer?.Dispose();

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

            container.Register<IAppDataPathProvider, DefaultAppDataPathProvider>();
            container.Register<IErrorHandler, UIErrorHandler>();
            container.Register<IRepositoryActionProvider, MacRepositoryActionProvider>();
            container.Register<IRepositoryReader, DefaultRepositoryReader>();
            container.Register<IRepositoryWriter, DefaultRepositoryWriter>();
            container.Register<IRepositoryStore, DefaultRepositoryStore>();
            container.Register<IPathProvider, MacDriveEnumerator>();
            container.Register<IPathCrawler, GravellPathCrawler>();
            container.Register<IPathSkipper, MacPathSkipper>();
            container.Register<IThreadDispatcher, MacThreadDispatcher>().AsSingleton();
            container.Register<IGitCommander, MacGitCommander>();
            container.Register<IAppSettingsProvider, FileAppSettingsProvider>();
        }

        private void UseRepositoryMonitor(TinyIoCContainer container)
        {
            var repositoryInformationAggregator = container.Resolve<IRepositoryInformationAggregator>();

            _repositoryMonitor = container.Resolve<IRepositoryMonitor>();
            _repositoryMonitor.Observe();
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

        private void ListenForSocketRequests()
        {
            _socketServer = new ResponseSocket(Ipc.RepoZIpcEndpoint.Address);

            while (true)
            {
                bool hasMore;
                var load = _socketServer.ReceiveFrameBytes(out hasMore);

                string message = Encoding.UTF8.GetString(load);

                if (string.IsNullOrEmpty(message))
                    return;

                if (message.StartsWith("list:", StringComparison.Ordinal))
                {
                    string repositoryNamePattern = message.Substring("list:".Length);

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

                    _socketServer.SendFrame(Encoding.UTF8.GetBytes(answer));
                }

                Thread.Sleep(100);
            }
        }

        public static AvailableVersion AvailableUpdate { get; private set; }
    }
}
