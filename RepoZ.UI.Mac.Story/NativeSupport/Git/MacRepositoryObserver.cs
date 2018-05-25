using CoreServices;
using RepoZ.Api.Git;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RepoZ.UI.Mac.Story.NativeSupport.Git
{
	public class MacRepositoryObserver : IRepositoryObserver
	{
		private Repository _repository;
        private FSEventStream _eventStream;
		private bool _ioDetected;

		public Action<Repository> OnChange { get; set; }

		public void Setup(Repository repository, int detectionToAlertDelayMilliseconds)
		{
            _repository = repository;

            _eventStream = new FSEventStream(new string[] { _repository.Path },
                                             TimeSpan.FromMilliseconds(100),
                                             FSEventStreamCreateFlags.FileEvents);

            _eventStream.ScheduleWithRunLoop(Foundation.NSRunLoop.Main);

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
                                        .ToArray();

            if (interestingEvents.Any())
                PauseWatcherAndScheduleCallback();
        }

		private void PauseWatcherAndScheduleCallback()
		{
			if (!_ioDetected)
			{
				_ioDetected = true;

				// stop the watcher once we found IO ...
				Stop();

				// ... and schedule a method to reactivate the watchers again
				// if nothing happened in between (regarding IO) it should also fire the OnChange-event
				Task.Run(() =>
					Thread.Sleep(DetectionToAlertDelayMilliseconds))
					.ContinueWith(AwakeWatcherAndScheduleEventInvocationIfNoFurtherIOGetsDetected);
			}
		}

		private void AwakeWatcherAndScheduleEventInvocationIfNoFurtherIOGetsDetected(object state)
		{
			if (_ioDetected)
			{
				// reset the flag, wait for further IO ...
				_ioDetected = false;
				Start();

				// ... and if nothing happened during the delay, invoke the OnChange-event
				Task.Run(() =>
					Thread.Sleep(DetectionToAlertDelayMilliseconds))
					.ContinueWith(t =>
					{
						if (_ioDetected)
							return;

						Console.WriteLine($"ONCHANGE on {_repository.Name}");
						OnChange?.Invoke(_repository);
					});
			}
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
