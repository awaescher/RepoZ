using AppKit;
using CoreServices;
using RepoZ.Api.Git;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RepoZ.App.Mac.NativeSupport.Git
{
    public class MacRepositoryDetector : IRepositoryDetector, IDisposable
    {
        private const string HEAD_LOG_FILE = @".git/logs/HEAD";

        private FSEventStream _eventStream;
        private IRepositoryReader _repositoryReader;

        public MacRepositoryDetector(IRepositoryReader repositoryReader)
        {
            _repositoryReader = repositoryReader;
        }

        public Action<Repository> OnAddOrChange { get; set; }
        public Action<string> OnDelete { get; set; }

        public void Setup(string path, int detectionToAlertDelayMilliseconds)
        {
            _eventStream = new FSEventStream(new string[] { path },
                                             TimeSpan.FromMilliseconds(100),
                                             FSEventStreamCreateFlags.FileEvents);

            _eventStream.ScheduleWithRunLoop(Foundation.NSRunLoop.Current);

            DetectionToAlertDelayMilliseconds = detectionToAlertDelayMilliseconds;

            _eventStream.Events += eventStream_Events;
        }

        public void Start()
        {
            _eventStream.Start();
        }

        public void Stop()
        {
            _eventStream.Stop();
        }

        void eventStream_Events(object sender, FSEventStreamEventsArgs args)
        {
            var interestingFlags = new FSEventStreamEventFlags[]
            {
                FSEventStreamEventFlags.ItemCreated,
                FSEventStreamEventFlags.ItemModified,
                FSEventStreamEventFlags.ItemRemoved,
                FSEventStreamEventFlags.ItemRenamed
            };

            var interestingEvents = args.Events
                                        .Where(e => interestingFlags.Any(f => e.Flags.HasFlag(f)))
                                        .Where(e => IsHead(e.Path))
                                        .ToArray();

            foreach (var ev in interestingEvents)
            {
                if (ev.Flags.HasFlag(FSEventStreamEventFlags.ItemCreated))
                    Task.Run(() => Task.Delay(DetectionToAlertDelayMilliseconds)).ContinueWith(t => EatRepo(ev.Path));
                else if (ev.Flags.HasFlag(FSEventStreamEventFlags.ItemModified))
                    EatRepo(ev.Path);
                else if (ev.Flags.HasFlag(FSEventStreamEventFlags.ItemRemoved))
                    NotifyHeadDeletion(ev.Path);
                else if (ev.Flags.HasFlag(FSEventStreamEventFlags.ItemRenamed))
                    NotifyHeadDeletion(ev.Path);
            }
        }

        private bool IsHead(string path)
        {
            int index = GetGitPathEndFromHeadFile(path);
            return index == (path.Length - HEAD_LOG_FILE.Length);
        }

        private string GetRepositoryPathFromHead(string headFile)
        {
            int end = GetGitPathEndFromHeadFile(headFile);

            if (end < 0)
                return string.Empty;

            return headFile.Substring(0, end);
        }

        private int GetGitPathEndFromHeadFile(string path) => path.IndexOf(HEAD_LOG_FILE, StringComparison.OrdinalIgnoreCase);

        private void EatRepo(string path)
        {
            var repo = _repositoryReader.ReadRepository(path);

            if (repo?.WasFound ?? false)
                OnAddOrChange?.Invoke(repo);
        }

        private void NotifyHeadDeletion(string headFile)
        {
            string path = GetRepositoryPathFromHead(headFile);
            if (!string.IsNullOrEmpty(path))
                OnDelete?.Invoke(path);
        }

        public void Dispose()
        {
            if (_eventStream != null)
            {
                _eventStream.Events -= eventStream_Events;
                _eventStream.Dispose();
            }
        }

        public int DetectionToAlertDelayMilliseconds { get; private set; }
    }
}
