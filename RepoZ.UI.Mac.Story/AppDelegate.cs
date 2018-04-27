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
using RepoZ.UI.Mac.Story.NativeSupport;
using RepoZ.UI.Mac.Story.NativeSupport.Git;
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
        private NSObject _eventMonitor;

        public override void DidFinishLaunching(NSNotification notification)
        {
            _statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
            _statusItem.Image = new NSImage("StatusBarImage.png");
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
        }

        public override void WillTerminate(NSNotification notification)
        {
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

            _repositoryMonitor.OnChangeDetected += (sender, repo) =>
            {
                repositoryInformationAggregator.Add(repo);
            };

            _repositoryMonitor.OnDeletionDetected += (sender, repoPath) =>
            {
                repositoryInformationAggregator.RemoveByPath(repoPath);
            };

            _repositoryMonitor.Observe();
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
    }
}
