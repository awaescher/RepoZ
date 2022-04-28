[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace RepoZ.App.Win
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using RepoZ.Api.Common;
    using RepoZ.Api.Common.Git;
    using RepoZ.Api.Common.IO;
    using RepoZ.Api.Git;
    using RepoZ.Api.IO;
    using RepoZ.Api.Win.IO;
    using RepoZ.Api.Win.PInvoke.Explorer;
    using Hardcodet.Wpf.TaskbarNotification;
    using LuceneSearch;
    using RepoZ.Api.Common.Common;
    using RepoZ.Api.Common.Git.AutoFetch;
    using RepoZ.Api.Common.Git.ProcessExecution;
    using RepoZ.Ipc;
    using RepoZ.App.Win.i18n;
    using RepoZ.Api;
    using SimpleInjector;

    /// <summary>
/// Interaction logic for App.xaml
/// </summary>
    public partial class App : Application, IRepositorySource
    {
        private static Timer _explorerUpdateTimer;
        private static Timer _updateTimer;
        private HotKey _hotkey;
        private static WindowsExplorerHandler _explorerHandler;
        private static IRepositoryMonitor _repositoryMonitor;
        private TaskbarIcon _notifyIcon;
        private IpcServer _ipcServer;
        private IAppSettingsService _settings;
        private static Container _container;

        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            // see: https://stackoverflow.com/a/520334/704281
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));

            Application.Current.Resources.MergedDictionaries[0] = ResourceDictionaryTranslationService.ResourceDictionary;

            _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            _container = new Container();

            RegisterServices(_container);

            StartModules(_container);

            UseRepositoryMonitor(_container);
            UseExplorerHandler(_container);
            PreloadRepositoryActions(_container);

            _container.Verify(VerificationOption.VerifyAndDiagnose);

            _updateTimer = new Timer(async state => await CheckForUpdatesAsync(), null, 5000, Timeout.Infinite);

            // We noticed that the hotkey registration causes a high CPU utilization if the window was not shown before.
            // To fix this, we need to make the window visible in EnsureWindowHandle() but we set the opacity to 0.0 to prevent flickering
            MainWindow window = _container.GetInstance<MainWindow>();
            EnsureWindowHandle(window);

            _hotkey = new HotKey(47110815);
            _hotkey.Register(window, HotKey.VK_R, HotKey.MOD_ALT | HotKey.MOD_CTRL, OnHotKeyPressed);

            _ipcServer = new IpcServer(new DefaultIpcEndpoint(), this);
            _ipcServer.Start();


            _settings = _container.GetInstance<IAppSettingsService>();

            if (_settings.MenuWidth > 0)
            {
                window.Width = _settings.MenuWidth;
            }

            if (_settings.MenuHeight > 0)
            {
                window.Height = _settings.MenuHeight;
            }

            window.SizeChanged += WindowOnSizeChanged;
        }

        private void WindowOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // persist
            _settings.MenuWidth = e.NewSize.Width;
            _settings.MenuHeight = e.NewSize.Height;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _container.GetInstance<MainWindow>().SizeChanged -= WindowOnSizeChanged;
            _ipcServer?.Stop();
            _ipcServer?.Dispose();

            _hotkey.Unregister();

            _explorerUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);

            var explorerHandler = _container.GetInstance<WindowsExplorerHandler>();
            explorerHandler.CleanTitles();

#pragma warning disable CA1416 // Validate platform compatibility
            _notifyIcon.Dispose();
#pragma warning restore CA1416 // Validate platform compatibility

            base.OnExit(e);
        }

        protected static void RegisterServices(Container container)
        {
            container.Register<MainWindow>(Lifestyle.Singleton);
            container.Register<StatusCharacterMap>(Lifestyle.Singleton);
            container.Register<StatusCompressor>(Lifestyle.Singleton);
            container.Register<IRepositoryInformationAggregator, DefaultRepositoryInformationAggregator>(Lifestyle.Singleton);
            container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>(Lifestyle.Singleton);
            container.Register<WindowsExplorerHandler>(Lifestyle.Singleton);
            container.Register<IRepositoryDetectorFactory, DefaultRepositoryDetectorFactory>(Lifestyle.Singleton);
            container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>(Lifestyle.Singleton);
            container.Register<IGitRepositoryFinderFactory, GitRepositoryFinderFactory>(Lifestyle.Singleton);
            container.Register<IAppDataPathProvider, DefaultAppDataPathProvider>(Lifestyle.Singleton);
            container.Register<IErrorHandler, UIErrorHandler>(Lifestyle.Singleton);
            container.Register<IRepositoryActionProvider, DefaultRepositoryActionProvider>(Lifestyle.Singleton);
            container.Register<IRepositoryReader, DefaultRepositoryReader>(Lifestyle.Singleton);
            container.Register<IRepositoryWriter, DefaultRepositoryWriter>(Lifestyle.Singleton);
            container.Register<IRepositoryStore, DefaultRepositoryStore>(Lifestyle.Singleton);
            container.Register<IPathProvider, DefaultDriveEnumerator>(Lifestyle.Singleton);
            container.Register<IPathSkipper, WindowsPathSkipper>(Lifestyle.Singleton);
            container.Register<IThreadDispatcher, WpfThreadDispatcher>(Lifestyle.Singleton);
            container.Register<IGitCommander, ProcessExecutingGitCommander>(Lifestyle.Singleton);
            container.Register<IAppSettingsService, FileAppSettingsService>(Lifestyle.Singleton);
            container.Register<IAutoFetchHandler, DefaultAutoFetchHandler>(Lifestyle.Singleton);
            container.Register<IRepositoryIgnoreStore, DefaultRepositoryIgnoreStore>(Lifestyle.Singleton);
            container.Register<IRepositoryActionConfigurationStore, DefaultRepositoryActionConfigurationStore>(Lifestyle.Singleton);
            container.Register<ITranslationService, ResourceDictionaryTranslationService>(Lifestyle.Singleton);
            container.Register<IRepositoryTagsResolver, DefaultRepositoryTagsResolver>(Lifestyle.Singleton);

            LuceneSearch.Registrations.Register(container);
        }

        private static void StartModules(Container container)
        {
            IEnumerable<IModule> modules = container.GetAllInstances<IModule>();
            var allTasks = Task.WhenAll(modules.Select(x => x.StartAsync()));
            allTasks.GetAwaiter().GetResult();
        }

        protected static void UseRepositoryMonitor(Container container)
        {
            var repositoryInformationAggregator = container.GetInstance<IRepositoryInformationAggregator>();
            _repositoryMonitor = container.GetInstance<IRepositoryMonitor>();
            _repositoryMonitor.Observe();
        }

        protected static void UseExplorerHandler(Container container)
        {
            _explorerHandler = container.GetInstance<WindowsExplorerHandler>();
            _explorerUpdateTimer = new Timer(RefreshTimerCallback, null, 1000, Timeout.Infinite);
        }

        protected static void PreloadRepositoryActions(Container container)
        {
            var store = container.GetInstance<IRepositoryActionConfigurationStore>();
            store.Preload();
        }

        private async Task CheckForUpdatesAsync()
        {
            await Task.Yield();
            AvailableUpdate = null;
            _updateTimer.Change((int)TimeSpan.FromHours(2).TotalMilliseconds, Timeout.Infinite);
        }

        protected static void RefreshTimerCallback(object state)
        {
            _explorerHandler.UpdateTitles();
            _explorerUpdateTimer.Change(500, Timeout.Infinite);
        }

        private void EnsureWindowHandle(Window window)
        {
            // We noticed that the hotkey registration at app start causes a high CPU utilization if the main window was not shown before.
            // To fix this, we need to make the window visible. However, to prevent flickering we move the window out of the screen bounds to show and hide it.

            window.Left = -9999;
            window.Show();
            window.Hide();
        }

        private void OnHotKeyPressed()
        {
            (Application.Current.MainWindow as MainWindow)?.ShowAndActivate();
        }

        public Ipc.Repository[] GetMatchingRepositories(string repositoryNamePattern)
        {
            var aggregator = _container.GetInstance<IRepositoryInformationAggregator>();
            return aggregator.Repositories
                             .Where(r => r.MatchesRegexFilter(repositoryNamePattern))
                             .Select(r => new Ipc.Repository
                                 {
                                     Name = r.Name,
                                     BranchWithStatus = r.BranchWithStatus,
                                     HasUnpushedChanges = r.HasUnpushedChanges,
                                     Path = r.Path,
                                 })
                             .ToArray();
        }

        public static string AvailableUpdate { get; private set; }
    }
}