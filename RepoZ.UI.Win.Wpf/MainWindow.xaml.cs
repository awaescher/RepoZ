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
using RepoZ.Api.Git;
using RepoZ.Api.Win.Git;
using TinyIoC;

namespace RepoZ.UI.Win.Wpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		private IRepositoryActionProvider _repositoryActionProvider;

		public MainWindow()
		{
			InitializeComponent();

			var container = TinyIoCContainer.Current;
			var statusCharacterMap = container.Resolve<StatusCharacterMap>();
			var aggregator = container.Resolve<IRepositoryInformationAggregator>();

			var monitor = container.Resolve<IRepositoryMonitor>() as DefaultRepositoryMonitor;
			if (monitor != null)
			{
				monitor.OnScanStateChanged = (scanning) => Dispatcher.Invoke(() => ShowScanningState(scanning));
				ShowScanningState(monitor.Scanning);
			}

			_repositoryActionProvider = container.Resolve<IRepositoryActionProvider>();

			lstRepositories.ItemsSource = aggregator.Repositories;


			lstRepositories.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(RepositoryView.Name), System.ComponentModel.ListSortDirection.Ascending));

			txtHelp.Text = GetHelp(statusCharacterMap);

			PlaceFormToLowerRight();
		}

		protected override void OnStateChanged(EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
				HideToTray();

			base.OnStateChanged(e);
		}

		private void lstRepositories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var repo = lstRepositories.SelectedItem as RepositoryView;
			if (repo == null || !repo.WasFound)
				return;

			var action = _repositoryActionProvider.GetFor(repo.Repository)
						 .FirstOrDefault(a => a.IsDefault);

			action?.Action?.Invoke(sender, e);
		}

		private void lstRepositories_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			var repo = lstRepositories.SelectedItem as RepositoryView;
			if (repo == null || !repo.WasFound)
			{
				e.Handled = true;
				return;
			}

			var items = ((FrameworkElement)e.Source).ContextMenu.Items;
			items.Clear();

			foreach (var action in _repositoryActionProvider.GetFor(repo.Repository))
			{
				if (action.BeginGroup && items.Count > 0)
					items.Add(new Separator());

				items.Add(CreateMenuItem(sender, action));
			}
		}

		private void HelpButton_Click(object sender, RoutedEventArgs e)
		{
			transitionerMain.SelectedIndex = (transitionerMain.SelectedIndex == 0 ? 1 : 0);
		}

		private void trayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
		{
			ShowFromTray();
		}

		private void ShowFromTray()
		{
			Show();

			if (WindowState == WindowState.Minimized)
				WindowState = WindowState.Normal;

			Activate();

			trayIcon.Visibility = Visibility.Hidden;
		}

		private void HideToTray()
		{
			Hide();
			trayIcon.Visibility = Visibility.Visible;
		}

		private void PlaceFormToLowerRight()
		{
			Top = SystemParameters.WorkArea.Height - Height - 1;
			Left = SystemParameters.WorkArea.Width - Width - 1;
		}

		private MenuItem CreateMenuItem(object sender, RepositoryAction action)
		{
			Action<object, object> clickAction = (object clickSender, object clickArgs) =>
			{
				var coords = new float[] { 0, 0 };
				if (action?.Action != null)
					action.Action(null, coords);
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

		private void ShowScanningState(bool isScanning)
		{
			this.Title = "RepoZ" + (isScanning ? " (scanning ...)" : "");
		}

		private string GetHelp(StatusCharacterMap statusCharacterMap)
		{
			return $@"
RepoZ is showing all Git repositories found on local drives. Each repository is listed with a status string which could look like this:

    S   +A   ~B   -C   |   +D   ~E   -F 

S
Represents the branch status in relation to remote (tracked origin) branch.
Note: This information reflects the state of the remote tracked branch after the last git fetch/pull of the remote.

{statusCharacterMap.IdenticalSign}
The local branch in at the same commit level as the remote branch.

{statusCharacterMap.ArrowUpSign}<num>
The local branch is ahead of the remote branch by the specified number of commits; a 'git push' is required to update the remote branch.

{statusCharacterMap.ArrowDownSign}<num>
The local branch is behind the remote branch by the specified number of commits; a 'git pull' is required to update the local branch.

{statusCharacterMap.NoUpstreamSign}
The branch is a local branch with no upstream branch information.

ABC represent the index; DEF represent the working directory
  + = Added files
  ~ = Modified files
  - = Removed files

Note that the status might be shorter if possible to improve readablility.

";
		}

	}
}
