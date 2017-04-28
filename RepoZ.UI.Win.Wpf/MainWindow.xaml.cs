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

			var aggregator = TinyIoCContainer.Current.Resolve<IRepositoryInformationAggregator>();
			lstRepositories.ItemsSource = aggregator.Repositories;

			_repositoryActionProvider = TinyIoCContainer.Current.Resolve<IRepositoryActionProvider>();

			lstRepositories.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(RepositoryView.Name), System.ComponentModel.ListSortDirection.Ascending));
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
	}
}
