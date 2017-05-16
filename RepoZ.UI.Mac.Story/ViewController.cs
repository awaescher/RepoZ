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
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var container = TinyIoCContainer.Current;

            var aggregator = container.Resolve<IRepositoryInformationAggregator>();
            var monitor = container.Resolve<IRepositoryMonitor>();

            aggregator.Repositories.Add(new RepositoryView(new Repository() { Name = "Test1", AheadBy=1, BehindBy = 2, CurrentBranch = "mtx" }));

            // Do any additional setup after loading the view.
            var datasource = new RepositoryTableDataSource(aggregator.Repositories);
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
