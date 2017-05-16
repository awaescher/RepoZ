using System;
using TinyIoC;
using AppKit;
using Foundation;
using RepoZ.UI.Mac.Story.Model;
using RepoZ.Api.Git;

namespace RepoZ.UI.Mac.Story
{
    public partial class ViewController : NSViewController
    {
        private IRepositoryInformationAggregator _aggregator;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var container = TinyIoCContainer.Current;

            _aggregator = container.Resolve<IRepositoryInformationAggregator>();
            var monitor = container.Resolve<IRepositoryMonitor>();

            // Do any additional setup after loading the view.
            var datasource = new RepositoryTableDataSource(_aggregator.Repositories);
            RepositoryTable.DataSource = datasource;
            RepositoryTable.Delegate = new RepositoryTableDelegate(datasource);
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
    }
}
