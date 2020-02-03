﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Git;
using RepoZ.App.Win.Controls;
using RepoZ.App.Win.i18n;
using SourceChord.FluentWPF;

namespace RepoZ.App.Win
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly IRepositoryActionProvider _repositoryActionProvider;
		private readonly IRepositoryIgnoreStore _repositoryIgnoreStore;
		private readonly DefaultRepositoryMonitor _monitor;
		private readonly ITranslationService _translationService;
		private bool _closeOnDeactivate = true;

		public MainWindow(StatusCharacterMap statusCharacterMap,
			IRepositoryInformationAggregator aggregator,
			IRepositoryMonitor repositoryMonitor,
			IRepositoryActionProvider repositoryActionProvider,
			IRepositoryIgnoreStore repositoryIgnoreStore,
			IAppSettingsService appSettingsService,
			ITranslationService translationService)
		{
			_translationService = translationService;

			InitializeComponent();

			AcrylicWindow.SetAcrylicWindowStyle(this, AcrylicWindowStyle.None);

			DataContext = new MainWindowPageModel(appSettingsService);
			SettingsMenu.DataContext = DataContext; // this is out of the visual tree

			_monitor = repositoryMonitor as DefaultRepositoryMonitor;
			if (_monitor != null)
			{
				_monitor.OnScanStateChanged += OnScanStateChanged;
				ShowScanningState(_monitor.Scanning);
			}

			_repositoryActionProvider = repositoryActionProvider ?? throw new ArgumentNullException(nameof(repositoryActionProvider));
			_repositoryIgnoreStore = repositoryIgnoreStore ?? throw new ArgumentNullException(nameof(repositoryIgnoreStore));
			lstRepositories.ItemsSource = aggregator.Repositories;
			var view = CollectionViewSource.GetDefaultView(lstRepositories.ItemsSource);
			view.CollectionChanged += View_CollectionChanged;
			view.Filter = FilterRepositories;

			lstRepositories.Items.SortDescriptions.Add(
				new SortDescription(nameof(RepositoryView.Name),
				ListSortDirection.Ascending));

			txtHelp.Text = GetHelp(statusCharacterMap);

			PlaceFormToLowerRight();
		}

		private void View_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			// use the list's itemsource directly, this one is not filtered (otherwise searching in the UI without matches could lead to the "no repositories yet"-screen)
			var hasRepositories = lstRepositories.ItemsSource.OfType<RepositoryView>().Any();
			tbNoRepositories.Visibility = hasRepositories ? Visibility.Hidden : Visibility.Visible;
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			ShowUpdateIfAvailable();
			txtFilter.Focus();
		}

		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);

			if (_closeOnDeactivate)
				Hide();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			Hide();
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			if (e.Key == Key.Escape)
			{
				bool isFilterActive = txtFilter.IsFocused && !string.IsNullOrEmpty(txtFilter.Text);
				if (!isFilterActive)
					Hide();
			}
		}

		public void ShowAndActivate()
		{
			Dispatcher.Invoke(() =>
			{
				PlaceFormToLowerRight();

				if (!IsShown)
					Show();

				Activate();
				txtFilter.Focus();
			});
		}

		private void OnScanStateChanged(object sender, bool isScanning)
		{
			Dispatcher.Invoke(() => ShowScanningState(isScanning));
		}

		private void lstRepositories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			// prevent doubleclicks from scrollbars and other non-data areas
			if (e.OriginalSource is Grid || e.OriginalSource is TextBlock)
				InvokeActionOnCurrentRepository();
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

		private void lstRepositories_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return || e.Key == Key.Enter)
				InvokeActionOnCurrentRepository();
		}

		private void InvokeActionOnCurrentRepository()
		{
			var selectedView = lstRepositories.SelectedItem as RepositoryView;
			if (selectedView == null || !selectedView.WasFound)
				return;

			RepositoryAction action;

			if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.LeftCtrl))
				action = _repositoryActionProvider.GetSecondaryAction(selectedView.Repository);
			else
				action = _repositoryActionProvider.GetPrimaryAction(selectedView.Repository);

			action?.Action?.Invoke(this, new EventArgs());
		}

		private void HelpButton_Click(object sender, RoutedEventArgs e)
		{
			transitionerMain.SelectedIndex = (transitionerMain.SelectedIndex == 0 ? 1 : 0);
		}

		private void MenuButton_Click(object sender, RoutedEventArgs e)
		{
			MenuButton.ContextMenu.IsOpen = true;
		}

		private void ScanButton_Click(object sender, RoutedEventArgs e)
		{
			_monitor.ScanForLocalRepositoriesAsync();
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			_monitor.Reset();
		}

		private void ResetIgnoreRulesButton_Click(object sender, RoutedEventArgs e)
		{
			_repositoryIgnoreStore.Reset();
		}

		private void UpdateButton_Click(object sender, RoutedEventArgs e)
		{
			bool hasLink = !string.IsNullOrWhiteSpace(App.AvailableUpdate?.Url);
			if (hasLink)
				Navigate(App.AvailableUpdate.Url);
		}

		private void StarButton_Click(object sender, RoutedEventArgs e)
		{
			Navigate("https://github.com/awaescher/RepoZ");
		}

		private void FollowButton_Click(object sender, RoutedEventArgs e)
		{
			Navigate("https://twitter.com/Waescher");
		}

		private void CoffeeButton_Click(object sender, RoutedEventArgs e)
		{
			Navigate("https://www.buymeacoffee.com/awaescher");
		}

		private void Navigate(string url)
		{
			Process.Start(url);
		}

		private void PlaceFormToLowerRight()
		{
			Top = SystemParameters.WorkArea.Height - Height;
			Left = SystemParameters.WorkArea.Width - Width;
		}

		private void ShowUpdateIfAvailable()
		{
			var updateHint = _translationService.Translate("Update hint", App.AvailableUpdate?.ShortestVersionString ?? "?.?");

			UpdateButton.Visibility = App.AvailableUpdate == null ? Visibility.Hidden : Visibility.Visible;
			UpdateButton.ToolTip = App.AvailableUpdate == null ? "" : updateHint;
			UpdateButton.Tag = App.AvailableUpdate;

			var parent = (Grid)UpdateButton.Parent;
			parent.ColumnDefinitions[Grid.GetColumn(UpdateButton)].Width = App.AvailableUpdate == null ? new GridLength(0) : GridLength.Auto;
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

			var item = new AcrylicMenuItem()
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
			ScanMenuItem.IsEnabled = !isScanning;
			ScanMenuItem.Header = isScanning
				? _translationService.Translate("Scanning")
				: _translationService.Translate("ScanComputer");
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				txtFilter.Focus();
				txtFilter.SelectAll();
			}

			if (e.Key == Key.Down && txtFilter.IsFocused)
				lstRepositories.Focus();

			// show/hide the titlebar to move the window for screenshots, for example
			if (e.Key == Key.F11)
			{
				var currentStyle = AcrylicWindow.GetAcrylicWindowStyle(this);
				var newStyle = currentStyle == AcrylicWindowStyle.None ? AcrylicWindowStyle.Normal : AcrylicWindowStyle.None;
				AcrylicWindow.SetAcrylicWindowStyle(this, newStyle);
			}

			// keep window open on deactivate to make screeshots, for example
			if (e.Key == Key.F12)
				_closeOnDeactivate = !_closeOnDeactivate;
		}

		private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			CollectionViewSource.GetDefaultView(lstRepositories.ItemsSource).Refresh();
		}

		private bool FilterRepositories(object item)
		{
			return (item as RepositoryView)?.MatchesFilter(txtFilter.Text) ?? false;
		}

		private void txtFilter_Finish(object sender, EventArgs e)
		{
            lstRepositories.Focus();
            if (lstRepositories.Items.Count > 0)
            {
                lstRepositories.SelectedIndex = 0;
                var item = (ListBoxItem)lstRepositories.ItemContainerGenerator.ContainerFromIndex(0);
                item?.Focus();
            }
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
				statusCharacterMap.StashSign
				);
		}

		public bool IsShown => Visibility == Visibility.Visible && IsActive;
	}
}
