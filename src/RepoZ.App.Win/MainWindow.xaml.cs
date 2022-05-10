namespace RepoZ.App.Win
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using RepoZ.Api;
    using RepoZ.Api.Common.Common;
    using RepoZ.Api.Common.Git;
    using RepoZ.Api.Git;
    using RepoZ.App.Win.Controls;
    using SourceChord.FluentWPF;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IRepositoryActionProvider _repositoryActionProvider;
        private readonly IRepositoryIgnoreStore _repositoryIgnoreStore;
        private readonly DefaultRepositoryMonitor _monitor;
        private readonly ITranslationService _translationService;
        private readonly IRepositoryActionConfigurationStore _actionConfigurationStore;
        private readonly IRepositorySearch _repositorySearch;
        private bool _closeOnDeactivate = true;
        private bool _refreshDelayed = false;
        private DateTime _timeOfLastRefresh = DateTime.MinValue;
        private readonly IFileSystem _fileSystem;

        public MainWindow(
            StatusCharacterMap statusCharacterMap,
            IRepositoryInformationAggregator aggregator,
            IRepositoryMonitor repositoryMonitor,
            IRepositoryActionProvider repositoryActionProvider,
            IRepositoryIgnoreStore repositoryIgnoreStore,
            IAppSettingsService appSettingsService,
            ITranslationService translationService,
            IRepositoryActionConfigurationStore actionConfigurationStore,
            IRepositorySearch repositorySearch,
            IFileSystem fileSystem)
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
            _actionConfigurationStore = actionConfigurationStore ?? throw new ArgumentNullException(nameof(actionConfigurationStore));
            _repositorySearch = repositorySearch ?? throw new ArgumentNullException(nameof(repositorySearch));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            lstRepositories.ItemsSource = aggregator.Repositories;

            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(aggregator.Repositories);
            ((ICollectionView)view).CollectionChanged += View_CollectionChanged;
            view.Filter = FilterRepositories;
            view.CustomSort = new CustomRepositoryViewSortBehavior();

            var appName = System.Reflection.Assembly.GetEntryAssembly().GetName();
            txtHelpCaption.Text = appName.Name + " " + appName.Version.ToString(2);
            txtHelp.Text = GetHelp(statusCharacterMap);

            PlaceFormByTaskbarLocation();
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
            txtFilter.SelectAll();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            if (_closeOnDeactivate)
            {
                Hide();
            }
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
                {
                    Hide();
                }
            }
        }

        public void ShowAndActivate()
        {
            Dispatcher.Invoke(() =>
                {
                    PlaceFormByTaskbarLocation();

                    if (!IsShown)
                    {
                        Show();
                    }

                    Activate();
                    txtFilter.Focus();
                    txtFilter.SelectAll();
                });
        }

        private void OnScanStateChanged(object sender, bool isScanning)
        {
            Dispatcher.Invoke(() => ShowScanningState(isScanning));
        }

        private void lstRepositories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // prevent doubleclicks from scrollbars and other non-data areas
            if (e.OriginalSource is Grid or TextBlock)
            {
                InvokeActionOnCurrentRepository();
            }
        }

        private void lstRepositories_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!LstRepositoriesContextMenuOpening(sender, ((FrameworkElement)e.Source).ContextMenu))
            {
                e.Handled = true;
            }
        }

        private bool LstRepositoriesContextMenuOpening(object sender, ContextMenu ctxMenu)
        {
            if (ctxMenu == null)
            {
                return false;
            }

            var selectedViews = lstRepositories.SelectedItems?.OfType<RepositoryView>();

            if (selectedViews == null || !selectedViews.Any())
            {
                return false;
            }

            var items = ctxMenu.Items;
            items.Clear();

            var innerRepositories = selectedViews.Select(view => view.Repository);
            foreach (var action in _repositoryActionProvider.GetContextMenuActions(innerRepositories))
            {
                if (action.BeginGroup && items.Count > 0)
                {
                    items.Add(new Separator());
                }

                items.Add(CreateMenuItem(sender, action, selectedViews));
            }

            return true;
        }

        private void lstRepositories_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Return or Key.Enter)
            {
                InvokeActionOnCurrentRepository();
            }
            else if (e.Key is Key.Left or Key.Right)
            {
                // try open context menu.
                var ctxMenu = ((FrameworkElement)e.Source).ContextMenu;
                if (ctxMenu != null && LstRepositoriesContextMenuOpening(sender, ctxMenu))
                {
                    ctxMenu.Placement = PlacementMode.Left;
                    ctxMenu.PlacementTarget = (UIElement)e.OriginalSource;
                    ctxMenu.IsOpen = true;
                }
            }
        }

        private void InvokeActionOnCurrentRepository()
        {
            if (lstRepositories.SelectedItem is not RepositoryView selectedView)
            {
                return;
            }

            if (!selectedView.WasFound)
            {
                return;
            }

            RepositoryAction action;

            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                action = _repositoryActionProvider.GetSecondaryAction(selectedView.Repository);
            }
            else
            {
                action = _repositoryActionProvider.GetPrimaryAction(selectedView.Repository);
            }

            action?.Action?.Invoke(this, EventArgs.Empty);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            transitionerMain.SelectedIndex = transitionerMain.SelectedIndex == 0 ? 1 : 0;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            MenuButton.ContextMenu.IsOpen = true;
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            _monitor.ScanForLocalRepositoriesAsync();
        }

        private void ReloadConfigButton_Click(object sender, RoutedEventArgs e)
        {
            _actionConfigurationStore.Preload();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _monitor.Reset();
        }

        private void ResetIgnoreRulesButton_Click(object sender, RoutedEventArgs e)
        {
            _repositoryIgnoreStore.Reset();
        }

        private void CustomizeContextMenu_Click(object sender, RoutedEventArgs e)
        {
            Navigate("https://github.com/awaescher/RepoZ-RepositoryActions");

            var fileName = ((FileRepositoryStore)_actionConfigurationStore).GetFileName();
            var directoryName = Path.GetDirectoryName(fileName);

            if (_fileSystem.Directory.Exists(directoryName))
            {
                Process.Start(new ProcessStartInfo(directoryName)
                    {
                        UseShellExecute = true,
                    });
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            bool hasLink = !string.IsNullOrWhiteSpace(App.AvailableUpdate);
            if (hasLink)
            {
                Navigate(App.AvailableUpdate);
            }
        }

        private void StarButton_Click(object sender, RoutedEventArgs e)
        {
            Navigate("https://github.com/awaescher/RepoZ");
        }

        private void FollowButton_Click(object sender, RoutedEventArgs e)
        {
            Navigate("https://twitter.com/Waescher");
        }

        private void SponsorButton_Click(object sender, RoutedEventArgs e)
        {
            Navigate("https://github.com/sponsors/awaescher");
        }

        private void Navigate(string url)
        {
            Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true,
                });
        }

        private void PlaceFormByTaskbarLocation()
        {
            var topY = SystemParameters.WorkArea.Top;
            var bottomY = SystemParameters.WorkArea.Height - Height;
            var leftX = SystemParameters.WorkArea.Left;
            var rightX = SystemParameters.WorkArea.Width - Width;

            switch (TaskbarLocator.GetTaskBarLocation())
            {
                case TaskbarLocator.TaskBarLocation.Top:
                    Top = topY;
                    Left = rightX;
                    break;
                case TaskbarLocator.TaskBarLocation.Bottom:
                    Top = bottomY;
                    Left = rightX;
                    break;
                case TaskbarLocator.TaskBarLocation.Left:
                    Top = bottomY;
                    Left = leftX;
                    break;
                case TaskbarLocator.TaskBarLocation.Right:
                    Top = bottomY;
                    Left = rightX;
                    break;
            }
        }

        private void ShowUpdateIfAvailable()
        {
            var updateHint = _translationService.Translate("Update hint", App.AvailableUpdate ?? "?.?");

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
                        var coords = new float[] { 0, 0, };

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
                    IsEnabled = action.CanExecute,
                };
            item.Click += new RoutedEventHandler(clickAction);

            // this is a deferred submenu. We want to make sure that the context menu can pop up
            // fast, while submenus are not evaluated yet. We don't want to make the context menu
            // itself slow because the creation of the submenu items takes some time.
            if (action.DeferredSubActionsEnumerator != null)
            {
                // this is a template submenu item to enable submenus under the current
                // menu item. this item gets removed when the real subitems are created
                item.Items.Add("");

                void SelfDetachingEventHandler(object _, RoutedEventArgs __)
                {
                    item.SubmenuOpened -= SelfDetachingEventHandler;
                    item.Items.Clear();

                    foreach (var subAction in action.DeferredSubActionsEnumerator())
                    {
                        item.Items.Add(CreateMenuItem(sender, subAction));
                    }

                    Console.WriteLine($"Added {item.Items.Count} deferred sub action(s).");
                }

                item.SubmenuOpened += SelfDetachingEventHandler;
            }

            return item;
        }

        private void SetViewsSynchronizing(IEnumerable<RepositoryView> affectedViews, bool synchronizing)
        {
            if (affectedViews == null)
            {
                return;
            }

            foreach (var view in affectedViews)
            {
                view.IsSynchronizing = synchronizing;
            }
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
            {
                lstRepositories.Focus();
            }

            // show/hide the titlebar to move the window for screenshots, for example
            if (e.Key == Key.F11)
            {
                AcrylicWindowStyle currentStyle = AcrylicWindow.GetAcrylicWindowStyle(this);
                AcrylicWindowStyle newStyle = currentStyle == AcrylicWindowStyle.None
                    ? AcrylicWindowStyle.Normal
                    : AcrylicWindowStyle.None;
                AcrylicWindow.SetAcrylicWindowStyle(this, newStyle);
            }

            // keep window open on deactivate to make screenshots, for example
            if (e.Key == Key.F12)
            {
                _closeOnDeactivate = !_closeOnDeactivate;
            }
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Text has changed, capture the timestamp
            if (sender != null)
            {
                _timeOfLastRefresh = DateTime.Now;
            }

            // Spin while updates are in progress
            if (DateTime.Now - _timeOfLastRefresh < TimeSpan.FromMilliseconds(100))
            {
                if (!_refreshDelayed)
                {
                    Dispatcher.InvokeAsync(async () =>
                        {
                            _refreshDelayed = true;
                            await Task.Delay(200);
                            _refreshDelayed = false;
                            txtFilter_TextChanged(null, e);
                        });
                }

                return;
            }

            // Refresh the view
            ICollectionView view = CollectionViewSource.GetDefaultView(lstRepositories.ItemsSource);
            view.Refresh();
        }

        private bool FilterRepositories(object item)
        {
            var query = txtFilter.Text;

            if (_refreshDelayed)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            query = query.Trim();

            if (query.StartsWith("!"))
            {
                var sanitizedQuery = query[1..];
                if (string.IsNullOrWhiteSpace(sanitizedQuery))
                {
                    return true;
                }

                var results = _repositorySearch.Search(sanitizedQuery).ToArray();
                return results.Contains((item as RepositoryView)?.Path);
            }
            else
            {
                return !_refreshDelayed && ((item as RepositoryView)?.MatchesFilter(txtFilter.Text) ?? false);
            }
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

    public class CustomRepositoryViewSortBehavior : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x is RepositoryView xView && y is RepositoryView yView)
            {
                return string.CompareOrdinal(xView.Name, yView.Name);
            }

            return 0;
        }
    }
}