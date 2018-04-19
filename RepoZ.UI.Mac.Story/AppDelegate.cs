using System;
using AppKit;
using Foundation;
using RepoZ.Api.Common;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Common.IO;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Mac;
using RepoZ.Api.Mac.Git;
using RepoZ.Api.Mac.IO;
using TinyIoC;

namespace RepoZ.UI.Mac.Story
{
    [Register("AppDelegate")]
    public partial class AppDelegate : NSApplicationDelegate, INSPopoverDelegate
    {
        NSStatusItem _statusItem;
        NSPopover _pop;
        public static NSViewController _ctrl;

        private IRepositoryInformationAggregator _aggregator;
        private IRepositoryMonitor _repositoryMonitor;

        public override void DidFinishLaunching(NSNotification notification)
        {
            // Insert code here to initialize your application
            _statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
            //statusItem.Menu = SystemTrayStatusMenu;
            // statusItem.View = new NSView();
            _statusItem.Title = "RepoZ";
            _statusItem.Target = this;
            _statusItem.Action = new ObjCRuntime.Selector("MenuAction");

            _pop = new NSPopover();
            _pop.Behavior = NSPopoverBehavior.ApplicationDefined;
            _pop.Delegate = this;

            //_pop.Show(new CoreGraphics.CGRect(100, 100, 100, 100), statusItem.Button, NSRectEdge.MinYEdge);

            var container = TinyIoCContainer.Current;

            RegisterServices(container);

            UseRepositoryMonitor(container);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }

        // Handler method definition
        [Export("MenuAction")]
        private void MenuAction()
        {
            _pop.ContentSize = new CoreGraphics.CGSize(100, 100);
            _pop.ContentViewController = new PopupViewController();
            _pop.Show(_statusItem.Button.Frame, _statusItem.Button, NSRectEdge.MaxYEdge);
        }

        private void RegisterServices(TinyIoCContainer container)
        {
            container.Register<IRepositoryInformationAggregator, DefaultRepositoryInformationAggregator>().AsSingleton();

            container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>().AsSingleton();
            container.Register<IRepositoryDetectorFactory, DefaultRepositoryDetectorFactory>().AsSingleton();
            container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>().AsSingleton();
            container.Register<IPathCrawlerFactory, DefaultPathCrawlerFactory>().AsSingleton();

            container.Register<IErrorHandler, UIErrorHandler>();
            container.Register<IRepositoryActionProvider, MacRepositoryActionProvider>();
            container.Register<IRepositoryReader, DefaultRepositoryReader>();
            container.Register<IRepositoryWriter, DefaultRepositoryWriter>();
            container.Register<IRepositoryStore, MacRepositoryStore>();
            container.Register<IPathProvider, MacDriveEnumerator>();
            container.Register<IPathCrawler, GravellPathCrawler>();
            container.Register<IPathSkipper, MacPathSkipper>();
            container.Register<IThreadDispatcher, MacThreadDispatcher>().AsSingleton();
            container.Register<IGitCommander, MacGitCommander>();
        }

        private void UseRepositoryMonitor(TinyIoCContainer container)
        {
            var repositoryInformationAggregator = container.Resolve<IRepositoryInformationAggregator>();

            _repositoryMonitor = container.Resolve<IRepositoryMonitor>();

            _repositoryMonitor.OnChangeDetected += (sender, repo) => repositoryInformationAggregator.Add(repo);
            _repositoryMonitor.OnDeletionDetected += (sender, repoPath) => repositoryInformationAggregator.RemoveByPath(repoPath);

            _repositoryMonitor.Observe();
        }
    }
}
