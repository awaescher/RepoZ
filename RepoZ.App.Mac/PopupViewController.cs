using System;
using Foundation;
using AppKit;
using RepoZ.App.Mac.Model;
using RepoZ.Api.Git;
using System.Diagnostics;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.Git.AutoFetch;
using RepoZ.Api.Common.Git;
using System.IO;
using RepoZ.App.Mac.Controls;

namespace RepoZ.App.Mac
{
	public partial class PopupViewController : AppKit.NSViewController
	{
		private const int MENU_MANAGE = 4711;
		private const int MENU_AUTOFETCH = 4712;
		private const int MENU_ADVANCED = 4713;
		private const int MENU_PINGBACK = 4714;

		private IRepositoryInformationAggregator _aggregator;
		private IRepositoryMonitor _monitor;
		private IAppSettingsService _appSettingsService;
		private IRepositoryIgnoreStore _repositoryIgnoreStore;
		private ITranslationService _translationService;
		private IRepositoryActionConfigurationStore _actionConfigurationStore;
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

			var container = TinyIoC.TinyIoCContainer.Current;


			_aggregator = container.Resolve<IRepositoryInformationAggregator>();
			var actionProvider = container.Resolve<IRepositoryActionProvider>();

			_monitor = container.Resolve<IRepositoryMonitor>();
			_appSettingsService = container.Resolve<IAppSettingsService>();
			_repositoryIgnoreStore = container.Resolve<IRepositoryIgnoreStore>();
			_translationService = container.Resolve<ITranslationService>();
			_actionConfigurationStore = container.Resolve<IRepositoryActionConfigurationStore>();

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

			SearchBox.PlaceholderString = _translationService.Translate("Search");
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

			var updateHint = _translationService.Translate("Update hint", AppDelegate.AvailableUpdate?.VersionString ?? "?.?");
			UpdateButton.ToolTip = hasUpdate ? updateHint : "";
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

			MenuButton.Menu = new NSMenu { AutoEnablesItems = false };

			var topLevelItems = new NSMenuItem[] {

				new NSMenuItem(_translationService.Translate("ManageRepos")) { Tag = MENU_MANAGE},
				new NSMenuItem(_translationService.Translate("AutoFetch")) { Tag = MENU_AUTOFETCH },
				new NSMenuItem(_translationService.Translate("Advanced")) { Tag = MENU_ADVANCED },
				NSMenuItem.SeparatorItem,
				new NSMenuItem(_translationService.Translate("PingBack")) { Tag = MENU_PINGBACK },
				NSMenuItem.SeparatorItem,
				new NSMenuItem(_translationService.Translate("Help"), (s, e) => ShowHelp()),
				new NSMenuItem(_translationService.Translate("Exit"), (s, e) => Quit())
			};

			foreach (var item in topLevelItems)
				MenuButton.Menu.AddItem(item);

			var manageItems = new NSMenuItem[] {
				new NSMenuItem(_translationService.Translate("ScanMac"), async (s, e) => await _monitor.ScanForLocalRepositoriesAsync()),
				new NSMenuItem(_translationService.Translate("Clear"), (s, e) => _monitor.Reset()),
				NSMenuItem.SeparatorItem,
				new NSMenuItem(_translationService.Translate("CustomizeRepositoryActions"), HandleCustomizeRepositoryAction),
				NSMenuItem.SeparatorItem,
				new NSMenuItem(_translationService.Translate("ResetIgnoreRules"), (s, e) => _repositoryIgnoreStore.Reset())
			};

			var autoFetchItems = new NSMenuItem[] {
				new CheckableMenuItem(
					_translationService.Translate(nameof(AutoFetchMode.Off)),
					() => _appSettingsService.AutoFetchMode == AutoFetchMode.Off,
					_ => _appSettingsService.AutoFetchMode = AutoFetchMode.Off),

				NSMenuItem.SeparatorItem,

				new CheckableMenuItem(_translationService.Translate(nameof(AutoFetchMode.Discretely)),
					() => _appSettingsService.AutoFetchMode == AutoFetchMode.Discretely,
					_ => _appSettingsService.AutoFetchMode = AutoFetchMode.Discretely),

				new CheckableMenuItem(_translationService.Translate(nameof(AutoFetchMode.Adequate)),
					() => _appSettingsService.AutoFetchMode == AutoFetchMode.Adequate,
					_ =>	_appSettingsService.AutoFetchMode = AutoFetchMode.Adequate),

				new CheckableMenuItem(_translationService.Translate(nameof(AutoFetchMode.Aggresive)),
					() => _appSettingsService.AutoFetchMode == AutoFetchMode.Aggresive,
					_ => _appSettingsService.AutoFetchMode = AutoFetchMode.Aggresive)
			};

			var manageRepositoriesItem = MenuButton.Menu.ItemWithTag(MENU_MANAGE);
			manageRepositoriesItem.Submenu = new NSMenu { AutoEnablesItems = false };

			foreach (var item in manageItems)
				manageRepositoriesItem.Submenu.AddItem(item);

			var autoFetchItem = MenuButton.Menu.ItemWithTag(MENU_AUTOFETCH);
			autoFetchItem.Submenu = new NSMenu { AutoEnablesItems = false };

			foreach (var item in autoFetchItems)
				autoFetchItem.Submenu.AddItem(item);

			var advancedItems = new NSMenuItem[] {
				new CheckableMenuItem(_translationService.Translate("PruneOnFetch"), () => _appSettingsService.PruneOnFetch, value => _appSettingsService.PruneOnFetch = value)
			};

			var advancedItem = MenuButton.Menu.ItemWithTag(MENU_ADVANCED);
			advancedItem.Submenu = new NSMenu { AutoEnablesItems = false };

			foreach (var item in advancedItems)
				advancedItem.Submenu.AddItem(item);

			var pingbackItems = new NSMenuItem[] {
				new NSMenuItem(_translationService.Translate("Donate"), (s, e) => Navigate("https://github.com/sponsors/awaescher")),
				NSMenuItem.SeparatorItem,
				new NSMenuItem(_translationService.Translate("Follow"), (s, e) => Navigate("https://twitter.com/Waescher")),
				new NSMenuItem(_translationService.Translate("Star"), (s, e) => Navigate("https://github.com/awaescher/RepoZ"))
			};

			var pingbackItem = MenuButton.Menu.ItemWithTag(MENU_PINGBACK);
			pingbackItem.Submenu = new NSMenu { AutoEnablesItems = false }; ;

			foreach (var item in pingbackItems)
				pingbackItem.Submenu.AddItem(item);
		}

		partial void MenuButton_Click(NSObject sender)
		{
			UpdateCheckableItems(MenuButton.Menu);

			MenuButton.Menu.PopUpMenu(null, new CoreGraphics.CGPoint() { X = 0, Y = MenuButton.Frame.Height }, MenuButton);
		}

		void UpdateCheckableItems(NSMenu menu)
		{
			if (menu is null)
				return;

			foreach (var item in menu.Items)
			{
				(item as CheckableMenuItem)?.Update();
				UpdateCheckableItems(item.Submenu);
			}
		}

		void HandleCustomizeRepositoryAction(object sender, EventArgs e)
		{
			Navigate("https://github.com/awaescher/RepoZ-RepositoryActions");

			var fileName = ((FileRepositoryStore)_actionConfigurationStore).GetFileName();
			var directoryName = Path.GetDirectoryName(fileName);

			if (Directory.Exists(directoryName))
				Process.Start(directoryName);
		}

		void Datasource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.InvokeOnMainThread(SetControlVisibilityByRepositoryAvailability);
		}

		private void SetControlVisibilityByRepositoryAvailability()
		{
			var unfilteredRepositoryCount = _aggregator.Repositories?.Count;
			var hasRepositories = unfilteredRepositoryCount > 0;
			lblNoRepositories.StringValue = _translationService.Translate("EmptyHint");
			lblNoRepositories.Hidden = hasRepositories;
			RepoTab.Hidden = !hasRepositories;
		}

		private void Quit()
		{
			NSApplication.SharedApplication.Terminate(this);
		}

		private void ShowHelp()
		{
			var appName = System.Reflection.Assembly.GetEntryAssembly().GetName();
			var bundleVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();

			var alert = new NSAlert
			{
				MessageText = appName.Name + " " + bundleVersion,
				AlertStyle = NSAlertStyle.Informational,
				InformativeText = GetHelp(TinyIoC.TinyIoCContainer.Current.Resolve<StatusCharacterMap>())
			};
			alert.AddButton(_translationService.Translate("GotIt"));
			alert.RunModal();
		}

		private string GetHelp(StatusCharacterMap statusCharacterMap)
		{
			return _translationService.Translate("Help Detail",
				statusCharacterMap.IdenticalSign,
				statusCharacterMap.StashSign,
				statusCharacterMap.IdenticalSign,
				statusCharacterMap.ArrowUpSign,
				statusCharacterMap.ArrowDownSign,
				statusCharacterMap.NoUpstreamSign,
				statusCharacterMap.StashSign);
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