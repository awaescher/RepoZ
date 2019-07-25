using System;
using Foundation;
using AppKit;
using RepoZ.App.Mac.Model;
using RepoZ.Api.Git;
using System.Diagnostics;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.Git.AutoFetch;

namespace RepoZ.App.Mac
{
    public partial class PopupViewController : AppKit.NSViewController
    {
        private const int MENU_MANAGE = 4711;
        private const int MENU_AUTOFETCH = 4712;
        private const int MENU_PINGBACK = 4713;

        private IRepositoryInformationAggregator _aggregator;
        private IRepositoryMonitor _monitor;
        private IAppSettingsService _appSettingsService;
        private IRepositoryIgnoreStore _repositoryIgnoreStore;
        private HeaderMetrics _initialHeaderMetrics;

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
            _appSettingsService = TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettingsService>();
            _repositoryIgnoreStore = TinyIoC.TinyIoCContainer.Current.Resolve<IRepositoryIgnoreStore>();

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

            CreateMenu();
        }

        public override void ViewWillDisappear()
        {
            UiStateHelper.Reset();

            base.ViewWillDisappear();
        }

        private void ShowUpdateIfAvailable()
        {
            if (_initialHeaderMetrics == null)
            {
                _initialHeaderMetrics = new HeaderMetrics()
                {
                    MenuButtonLeft = MenuButton.Frame.Left,
                    UpdateButtonLeft = UpdateButton.Frame.Left,
                    SearchBoxWidth = SearchBox.Frame.Width
                };
            }

            bool hasUpdate = AppDelegate.AvailableUpdate != null;

            // to debug
            //hasUpdate = true;

            UpdateButton.ToolTip = hasUpdate ? $"Version {AppDelegate.AvailableUpdate?.VersionString ?? "?.?"} is available." : "";
            UpdateButton.Hidden = !hasUpdate;

            if (UpdateButton.Hidden)
            {
                var additionalSpace = UpdateButton.Frame.Left - MenuButton.Frame.Left;
                MenuButton.Frame = UpdateButton.Frame;
                SearchBox.Frame = SearchBox.Frame.WithWidth(SearchBox.Frame.Width + additionalSpace);
            }
            else
            {
                UpdateButton.Frame = UpdateButton.Frame.WithLeft(_initialHeaderMetrics.UpdateButtonLeft);
                MenuButton.Frame = MenuButton.Frame.WithLeft(_initialHeaderMetrics.MenuButtonLeft);
                SearchBox.Frame = SearchBox.Frame.WithWidth(_initialHeaderMetrics.SearchBoxWidth);
            }
        }

        void SearchBox_FinishInput(object sender, EventArgs e)
        {
            string value = SearchBox.StringValue;

            this.View.Window.MakeFirstResponder(RepoTab);
            if (RepoTab.RowCount > 0)
                RepoTab.SelectRow(0, byExtendingSelection: false);
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
            dataSource.Filter(filterString);
        }

        partial void UpdateButton_Click(NSObject sender)
        {
            if (string.IsNullOrEmpty(AppDelegate.AvailableUpdate?.Url))
                return;

            Navigate(AppDelegate.AvailableUpdate.Url);
        }

        private void Navigate(string url)
        {
            Process.Start(url);
        }

        private void CreateMenu()
        {
            if (MenuButton.Menu != null)
                return;

            MenuButton.Menu = new NSMenu();

            var topLevelItems = new NSMenuItem[] {

                new NSMenuItem("Manage repositories") { Tag = MENU_MANAGE},
                new NSMenuItem("Auto fetch") { Tag = MENU_AUTOFETCH },
                NSMenuItem.SeparatorItem,
                new NSMenuItem("Ping back ♥︎") { Tag = MENU_PINGBACK },
                NSMenuItem.SeparatorItem,
                new NSMenuItem("Help", (s, e) => ShowHelp()),
                new NSMenuItem("Quit", (s, e) => Quit())
            };

            foreach (var item in topLevelItems)
                MenuButton.Menu.AddItem(item);

            var manageItems = new NSMenuItem[] {
                new NSMenuItem("Scan mac", async (s, e) => await _monitor.ScanForLocalRepositoriesAsync()),
                new NSMenuItem("Clear", (s, e) => _monitor.Reset()),
                NSMenuItem.SeparatorItem,
                new NSMenuItem("Reset ignore rules", (s, e) => _repositoryIgnoreStore.Reset())
            };

            var autoFetchItems = new NSMenuItem[] {
                new NSMenuItem(nameof(AutoFetchMode.Off), HandleAutoFetchChange) { Identifier = AutoFetchMode.Off.ToString() },
                new NSMenuItem(nameof(AutoFetchMode.Discretely), HandleAutoFetchChange) { Identifier = AutoFetchMode.Discretely.ToString() },
                new NSMenuItem(nameof(AutoFetchMode.Adequate), HandleAutoFetchChange) { Identifier = AutoFetchMode.Adequate.ToString() },
                new NSMenuItem(nameof(AutoFetchMode.Aggresive), HandleAutoFetchChange) { Identifier = AutoFetchMode.Aggresive.ToString() }
            };

            var manageRepositoriesItem = MenuButton.Menu.ItemWithTag(MENU_MANAGE);
            manageRepositoriesItem.Submenu = new NSMenu();

            foreach (var item in manageItems)
                manageRepositoriesItem.Submenu.AddItem(item);

            var autoFetchItem = MenuButton.Menu.ItemWithTag(MENU_AUTOFETCH);
            autoFetchItem.Submenu = new NSMenu();

            foreach (var item in autoFetchItems)
                autoFetchItem.Submenu.AddItem(item);

            var pingbackItems = new NSMenuItem[] {
                new NSMenuItem("Star RepoZ on GitHub", (s, e) => Navigate("https://github.com/awaescher/RepoZ")),
                new NSMenuItem("Follow @Waescher", (s, e) => Navigate("https://twitter.com/Waescher")),
                NSMenuItem.SeparatorItem,
                new NSMenuItem("Buy me a coffee", (s, e) => Navigate("https://www.buymeacoffee.com/awaescher"))
            };

            var pingbackItem = MenuButton.Menu.ItemWithTag(MENU_PINGBACK);
            pingbackItem.Submenu = new NSMenu();

            foreach (var item in pingbackItems)
                pingbackItem.Submenu.AddItem(item);
        }

        partial void MenuButton_Click(NSObject sender)
        {
            var currentMode = _appSettingsService.AutoFetchMode;
            var autoFetchItem = MenuButton.Menu.ItemWithTag(MENU_AUTOFETCH);

            for (int i = 0; i < autoFetchItem.Submenu.Count; i++)
            {
                var item = autoFetchItem.Submenu.ItemAt(i);
                var itemMode = (AutoFetchMode)Enum.Parse(typeof(AutoFetchMode), item.Identifier);
                item.State = itemMode == currentMode ? NSCellStateValue.On : NSCellStateValue.Off;
            }

            MenuButton.Menu.PopUpMenu(null, new CoreGraphics.CGPoint() { X = 0, Y = MenuButton.Frame.Height }, MenuButton);
        }

        void HandleAutoFetchChange(object sender, EventArgs e)
        {
            var newMode = (AutoFetchMode)Enum.Parse(typeof(AutoFetchMode), (sender as NSMenuItem).Title);
            _appSettingsService.AutoFetchMode = newMode;
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

        private void Quit()
        {
            NSApplication.SharedApplication.Terminate(this);
        }

        private void ShowHelp()
        {
            var alert = new NSAlert
            {
                MessageText = "How the read the status information",
                AlertStyle = NSAlertStyle.Informational,
                InformativeText = GetHelp(TinyIoC.TinyIoCContainer.Current.Resolve<StatusCharacterMap>())
            };
            alert.AddButton("Got it");
            var returnValue = alert.RunModal();
        }

        private string GetHelp(StatusCharacterMap statusCharacterMap)
        {
            return $@"
RepoZ is showing all git repositories found on local drives. Each repository is listed with a status string which could look like this:

    master  {statusCharacterMap.IdenticalSign}   +1   ~2   -3   |   +4   ~5   -6


master
... represents the current branch or the SHA of a detached HEAD.

{statusCharacterMap.IdenticalSign}
... represents the branch status in relation to its remote (tracked origin) branch.
In this case, the local branch is at the same commit level as the remote branch.

{statusCharacterMap.ArrowUpSign}<num>
... indicates that the local branch is ahead of the remote branch by the specified number of commits; a 'git push' is required to update the remote branch.

{statusCharacterMap.ArrowDownSign}<num>
... indicates that the local branch is behind the remote branch by the specified number of commits; a 'git pull' is required to update the local branch.

{statusCharacterMap.NoUpstreamSign}
... indicates that the local branch has no upstream branch. 'git push' needs to be called with '--set-upstream' to push changes to a remote branch.



The following numbers represent added (+1), modified (~2) and removed files (-3) from the index.
The numbers after the pipe sign represent added (+4), modified (~5) and removed files (-6) on the working directory.

Please note:
This information reflects the state of the remote tracked branch after the last ""git fetch"".
You can enable Auto fetch in the RepoZ menu to keep the information up to date.

Note that the status might be shorter if possible to improve readablility.
";
        }

        public new PopupView View => (PopupView)base.View;

        private class HeaderMetrics
        {
            public nfloat UpdateButtonLeft { get; set; }
            public nfloat MenuButtonLeft { get; set; }
            public nfloat SearchBoxWidth { get; set; }
        }
    }
}