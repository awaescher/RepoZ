using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Git;
using RepoZ.Api.Win.Git;
using TinyIoC;
using TinySoup;
using TinySoup.Identifier;
using TinySoup.Model;

namespace RepoZ.UI.Win.Wpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		private IRepositoryActionProvider _repositoryActionProvider;
		private DefaultRepositoryMonitor _monitor;

		public MainWindow(StatusCharacterMap statusCharacterMap,
			IRepositoryInformationAggregator aggregator,
			IRepositoryMonitor repositoryMonitor,
			IRepositoryActionProvider repositoryActionProvider)
		{
			InitializeComponent();

			_monitor = repositoryMonitor as DefaultRepositoryMonitor;
			if (_monitor != null)
			{
				_monitor.OnScanStateChanged += OnScanStateChanged;
				ShowScanningState(_monitor.Scanning);
			}

			_repositoryActionProvider = repositoryActionProvider;

			lstRepositories.ItemsSource = aggregator.Repositories;


			lstRepositories.Items.SortDescriptions.Add(
				new System.ComponentModel.SortDescription(nameof(RepositoryView.Name),
				System.ComponentModel.ListSortDirection.Ascending));

			txtHelp.Text = GetHelp(statusCharacterMap);

			PlaceFormToLowerRight();

			Task.Run(() => CheckForUpdatesAsync());
		}

		private async Task CheckForUpdatesAsync()
		{
			var request = new UpdateRequest()
				.WithNameAndVersionFromEntryAssembly()
				.AsAnonymousClient()
				.OnChannel("stable")
				.OnPlatform(new OperatingSystemIdentifier().WithSuffix("(WPF)"));

			var client = new WebSoupClient();
			var updates = await client.CheckForUpdatesAsync(request);

			var newest = updates.FirstOrDefault();
			if (newest != null)
				Dispatcher.Invoke((Action)(() => Title = newest.ToString()));
		}

		protected override void OnClosed(EventArgs e)
		{
			if (_monitor != null)
				_monitor.OnScanStateChanged -= OnScanStateChanged;

			base.OnClosed(e);
		}

		private void OnScanStateChanged(object sender, bool isScanning)
		{
			Dispatcher.Invoke(() => ShowScanningState(isScanning));
		}

		private void lstRepositories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var selectedView = lstRepositories.SelectedItem as RepositoryView;
			if (selectedView == null || !selectedView.WasFound)
				return;

			RepositoryAction action;

			if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.LeftCtrl))
				action = _repositoryActionProvider.GetSecondaryAction(selectedView.Repository);
			else
				action = _repositoryActionProvider.GetPrimaryAction(selectedView.Repository);

			action?.Action?.Invoke(sender, e);
		}

		private void lstRepositories_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			var selectedViews = lstRepositories.SelectedItems?.OfType<RepositoryView>();

			if (selectedViews == null || !selectedViews.Any())
			{
				e.Handled = true;
				return;
			}

			var items = ((FrameworkElement)e.Source).ContextMenu.Items;
			items.Clear();

			var innerRepositories = selectedViews.Select(view => view.Repository);
			foreach (var action in _repositoryActionProvider.GetContextMenuActions(innerRepositories))
			{
				if (action.BeginGroup && items.Count > 0)
					items.Add(new Separator());

				items.Add(CreateMenuItem(sender, action, selectedViews));
			}
		}

		private void HelpButton_Click(object sender, RoutedEventArgs e)
		{
			transitionerMain.SelectedIndex = (transitionerMain.SelectedIndex == 0 ? 1 : 0);
		}

		private void PlaceFormToLowerRight()
		{
			Top = SystemParameters.WorkArea.Height - Height - 1;
			Left = SystemParameters.WorkArea.Width - Width - 1;
		}

		private MenuItem CreateMenuItem(object sender, RepositoryAction action, IEnumerable<RepositoryView> affectedViews = null)
		{
			Action<object, object> clickAction = (object clickSender, object clickArgs) =>
			{
				if (action?.Action != null)
				{
					var coords = new float[] { 0, 0 };

					// run actions in the UI async to not block it
					if (action.ExecutionCausesSynchronizing)
					{
						Task.Run(() => SetViewsSynchronizing(affectedViews, true))
							.ContinueWith(t => action.Action(null, coords))
							.ContinueWith(t => SetViewsSynchronizing(affectedViews, false));
					}
					else
					{
						Task.Run(() => action.Action(null, coords));
					}
				}
			};

			var item = new MenuItem()
			{
				Header = action.Name,
				IsEnabled = action.CanExecute
			};
			item.Click += new RoutedEventHandler(clickAction);

			if (action.SubActions != null)
			{
				foreach (var subAction in action.SubActions)
					item.Items.Add(CreateMenuItem(sender, subAction));
			}

			return item;
		}

		private void SetViewsSynchronizing(IEnumerable<RepositoryView> affectedViews, bool synchronizing)
		{
			if (affectedViews == null)
				return;

			foreach (var view in affectedViews)
				view.IsSynchronizing = synchronizing;
		}

		private void ShowScanningState(bool isScanning)
		{
			this.Title = "RepoZ" + (isScanning ? " (scanning ...)" : "");
		}

		private string GetHelp(StatusCharacterMap statusCharacterMap)
		{
			return $@"
RepoZ is showing all git repositories found on local drives. Each repository is listed with a status string which could look like this:

    current_branch  [i]   +1   ~2   -3   |   +4   ~5   -6 


current_branch
The current branch or the SHA of a detached HEAD.

[i] Represents the branch status in relation to its remote (tracked origin) branch.

[i] =  {statusCharacterMap.IdenticalSign}
The local branch in at the same commit level as the remote branch.

[i] =  {statusCharacterMap.ArrowUpSign}<num>
The local branch is ahead of the remote branch by the specified number of commits; a 'git push' is required to update the remote branch.

[i] =  {statusCharacterMap.ArrowDownSign}<num>
The local branch is behind the remote branch by the specified number of commits; a 'git pull' is required to update the local branch.

[i] =  {statusCharacterMap.NoUpstreamSign}
The local branch has no upstream branch. 'git push' needs to be called with '--set-upstream' to push changes to a remote branch.

The following numbers represent added (+1), modified (~2) and removed files (-3) from the index.
The numbers after the pipe sign represent added (+4), modified (~5) and removed files (-6) on the working directory.

Please note:
This information reflects the state of the remote tracked branch after the last git fetch/pull of the remote.
Note that the status might be shorter if possible to improve readablility.

";
		}

	}
}
