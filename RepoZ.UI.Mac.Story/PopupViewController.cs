using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using RepoZ.UI.Mac.Story.Model;
using RepoZ.Api.Git;
using TinySoup.Model;
using TinySoup.Identifier;
using TinySoup;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RepoZ.UI.Mac.Story
{
	public partial class PopupViewController : AppKit.NSViewController
	{
       	private IRepositoryInformationAggregator _aggregator;

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

			// Do any additional setup after loading the view.
			var datasource = new RepositoryTableDataSource(_aggregator.Repositories);
			RepoTab.DataSource = datasource;
			RepoTab.Delegate = new RepositoryTableDelegate(RepoTab, datasource, actionProvider);

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

			Process.Start(AppDelegate.AvailableUpdate.Url);
		}

		public new PopupView View => (PopupView)base.View;
	}
}
