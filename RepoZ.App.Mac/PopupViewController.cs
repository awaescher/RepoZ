using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using RepoZ.App.Mac.Model;
using RepoZ.Api.Git;
using TinySoup.Model;
using TinySoup.Identifier;
using TinySoup;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RepoZ.App.Mac
{
    public partial class PopupViewController : AppKit.NSViewController
    {
        private IRepositoryInformationAggregator _aggregator;
        private IRepositoryMonitor _monitor;
        private StringCommandHandler _stringCommandHandler = new StringCommandHandler();

        #region Constructors

        // Called when created from unmanaged code
        public PopupViewController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public PopupViewController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public PopupViewController() : base("PopupView", NSBundle.MainBundle)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        protected override void Dispose(bool disposing)
        {
            SearchBox.FinishInput -= SearchBox_FinishInput;

            base.Dispose(disposing);
        }

        #endregion

        public override void ViewWillAppear()
        {
            base.ViewWillAppear();

            if (_aggregator != null)
                return;

            _aggregator = TinyIoC.TinyIoCContainer.Current.Resolve<IRepositoryInformationAggregator>();
            var actionProvider = TinyIoC.TinyIoCContainer.Current.Resolve<IRepositoryActionProvider>();

            _monitor = TinyIoC.TinyIoCContainer.Current.Resolve<IRepositoryMonitor>();
            _monitor.OnScanStateChanged += _monitor_OnScanStateChanged;

            _stringCommandHandler.Define(new string[] { "help", "man", "?" }, ShowCommandReference, "Shows this help page");
            _stringCommandHandler.Define(new string[] { "scan" }, async () => await _monitor.ScanForLocalRepositoriesAsync(), "Scans this Mac for git repositories");
            _stringCommandHandler.Define(new string[] { "reset" }, _monitor.Reset, "Resets the repository cache");
            _stringCommandHandler.Define(new string[] { "info", "stats" }, ShowStats, "Shows process informations");
            _stringCommandHandler.Define(new string[] { "quit", "close", "exit" }, () => NSApplication.SharedApplication.Terminate(this), "Closes the application");

            // Do any additional setup after loading the view.
            var datasource = new RepositoryTableDataSource(_aggregator.Repositories);
            RepoTab.DataSource = datasource;
            datasource.CollectionChanged += Datasource_CollectionChanged;
            RepoTab.Delegate = new RepositoryTableDelegate(RepoTab, datasource, actionProvider);
            SetControlVisibilityByRepositoryAvailability();

            RepoTab.BackgroundColor = NSColor.Clear;
            RepoTab.EnclosingScrollView.DrawsBackground = false;

            SearchBox.FinishInput += SearchBox_FinishInput;

            SearchBox.NextKeyView = RepoTab;
        }

        public override void ViewDidAppear()
        {
            // make sure the app gets focused directly when opened
            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);

            base.ViewDidAppear();

            ShowUpdateIfAvailable();
            this.View.Window.MakeFirstResponder(SearchBox);
        }

        public override void ViewWillDisappear()
        {
            UiStateHelper.Reset();

            base.ViewWillDisappear();
        }

        private void ShowUpdateIfAvailable()
        {
            UpdateButton.ToolTip = AppDelegate.AvailableUpdate == null ? "" : $"Version {AppDelegate.AvailableUpdate.VersionString} is available.";
            UpdateButton.Hidden = AppDelegate.AvailableUpdate == null;

            var newSearchBoxFrame = SearchBox.Frame;

            var availableWidth = UpdateButton.Hidden ? this.View.Frame.Width : UpdateButton.Frame.X;
            newSearchBoxFrame.Width = availableWidth - (SearchBox.Frame.X * 2);

            SearchBox.Frame = newSearchBoxFrame;
        }

        void SearchBox_FinishInput(object sender, EventArgs e)
        {
            string value = SearchBox.StringValue;

            if (_stringCommandHandler.IsCommand(value))
            {
                if (_stringCommandHandler.Handle(value))
                    SearchBox.StringValue = "";
                else
                    SearchBox.SelectText(this);
            }
            else
            {
                this.View.Window.MakeFirstResponder(RepoTab);
                if (RepoTab.RowCount > 0)
                    RepoTab.SelectRow(0, byExtendingSelection: false);
            }
        }

        public override void KeyDown(NSEvent theEvent)
        {
            if (theEvent.KeyCode == (ushort)NSKey.F && UiStateHelper.CommandKeyDown)
                this.View.Window.MakeFirstResponder(SearchBox);
            else
                base.KeyDown(theEvent);
        }

        public override void FlagsChanged(NSEvent theEvent)
        {
            base.FlagsChanged(theEvent);

            UiStateHelper.CommandKeyDown = theEvent.ModifierFlags.HasFlag(NSEventModifierMask.CommandKeyMask);
            UiStateHelper.OptionKeyDown = theEvent.ModifierFlags.HasFlag(NSEventModifierMask.AlternateKeyMask);
            UiStateHelper.ShiftKeyDown = theEvent.ModifierFlags.HasFlag(NSEventModifierMask.ShiftKeyMask);
            UiStateHelper.ControlKeyDown = theEvent.ModifierFlags.HasFlag(NSEventModifierMask.ControlKeyMask);
        }

        partial void SearchChanged(NSObject sender)
        {
            var dataSource = RepoTab.DataSource as RepositoryTableDataSource;
            var filterString = (sender as NSControl).StringValue;

            if (!_stringCommandHandler.IsCommand(filterString))
                dataSource.Filter(filterString);
        }

        partial void UpdateButton_Click(NSObject sender)
        {
            if (string.IsNullOrEmpty(AppDelegate.AvailableUpdate?.Url))
                return;

            Process.Start(AppDelegate.AvailableUpdate.Url);
        }

        void _monitor_OnScanStateChanged(object sender, bool e)
        {

        }

        void Datasource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.InvokeOnMainThread(SetControlVisibilityByRepositoryAvailability);
        }

        private void SetControlVisibilityByRepositoryAvailability()
        {
            var unfilteredRepositoryCount = _aggregator.Repositories?.Count;
            var hasRepositories = unfilteredRepositoryCount > 0;
            lblNoRepositories.Hidden = hasRepositories;
            RepoTab.Hidden = !hasRepositories;
        }

        private void ShowCommandReference()
        {
            var alert = new NSAlert
            {
                MessageText = _stringCommandHandler.GetHelpText(),
                AlertStyle = NSAlertStyle.Informational
            };
            alert.AddButton("OK");
            var returnValue = alert.RunModal();
        }
		
		private void ShowStats()
        {
            var process = Process.GetCurrentProcess();

            var stats = string.Join("", new string[] {
                "Version\t\t\t", NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString(),
                "\nRepositories\t\t", (_aggregator.Repositories?.Count ?? 0).ToString(),
                "\nProcess Id\t\t", process.Id.ToString(),
                "\nWorking Set\t\t", (process.WorkingSet64 / 1024 / 1024).ToString() + " MB",
                "\nRunning Since\t", (DateTime.UtcNow - process.StartTime.ToUniversalTime()).ToString("")
            });

            var alert = new NSAlert
            {
                MessageText = stats,
                AlertStyle = NSAlertStyle.Informational
            };
            alert.AddButton("OK");
            var returnValue = alert.RunModal();
        }

        public new PopupView View => (PopupView)base.View;
    }
}