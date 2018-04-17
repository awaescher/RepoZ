using System;
using TinyIoC;
using AppKit;
using Foundation;
using RepoZ.UI.Mac.Story.Model;
using RepoZ.Api.Git;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Common.IO;
using RepoZ.Api.Mac;
using RepoZ.Api.Mac.Git;
using RepoZ.Api.Mac.IO;
using RepoZ.Api.IO;
using RepoZ.Api.Common;

namespace RepoZ.UI.Mac.Story
{
    public partial class ViewController : NSViewController
    {
        private IRepositoryInformationAggregator _aggregator;
        private IRepositoryMonitor _repositoryMonitor;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var container = TinyIoCContainer.Current;

            RegisterServices(container);

            UseRepositoryMonitor(container);

            System.Threading.Tasks.Task.Factory.StartNew(() => System.Threading.Tasks.Task.Delay(5000).ContinueWith((t) =>
            {
                InvokeOnMainThread(() =>
                {
                    _aggregator = container.Resolve<IRepositoryInformationAggregator>();
                    var monitor = container.Resolve<IRepositoryMonitor>();

                    // Do any additional setup after loading the view.
                    var datasource = new RepositoryTableDataSource(_aggregator.Repositories);
                    RepositoryTable.DataSource = datasource;
                    RepositoryTable.Delegate = new RepositoryTableDelegate(RepositoryTable, datasource);
                });
            }));
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
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
